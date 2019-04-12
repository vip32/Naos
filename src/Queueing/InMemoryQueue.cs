namespace Naos.Core.Queueing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Queueing.Domain;

    public class InMemoryQueue<TData> : BaseQueue<TData, InMemoryQueueOptions>
        where TData : class
    {
        private readonly ConcurrentQueue<QueueItem<TData>> queue = new ConcurrentQueue<QueueItem<TData>>();
        private readonly ConcurrentDictionary<string, QueueItem<TData>> dequeued = new ConcurrentDictionary<string, QueueItem<TData>>();
        private readonly ConcurrentQueue<QueueItem<TData>> deadletterQueue = new ConcurrentQueue<QueueItem<TData>>();

        private int enqueuedCount;
        private int dequeuedCount;
        private int completedCount;
        private int abandonedCount;
        private int workerErrorCount;

        public InMemoryQueue()
            : this(o => o)
        {
        }

        public InMemoryQueue(InMemoryQueueOptions options)
            : base(options)
        {
        }

        public InMemoryQueue(Builder<InMemoryQueueOptionsBuilder, InMemoryQueueOptions> optionsBuilder)
            : this(optionsBuilder(new InMemoryQueueOptionsBuilder()).Build())
        {
        }

        public override async Task<string> EnqueueAsync(TData data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            this.EnsureMetaData(data);

            using(this.logger.BeginScope(new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = data.As<IHaveCorrelationId>()?.CorrelationId,
            }))
            {
                await this.EnsureQueueAsync().AnyContext();

                var item = new QueueItem<TData>(IdGenerator.Instance.Next, data.Clone(), this, DateTime.UtcNow, 0);
                this.logger.LogDebug($"queue item enqueue (id={item.Id}, queue={this.options.Name})");
                this.queue.Enqueue(item);

                Interlocked.Increment(ref this.enqueuedCount);

                this.logger.LogJournal(LogKeys.Queueing, $"item enqueued (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogEventPropertyKeys.TrackEnqueue);
                this.logger.LogTraceEvent(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceEventNames.Queue);
                this.LastEnqueuedDate = DateTime.UtcNow;
                return item.Id;
            }
        }

        public override Task RenewLockAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogDebug($"queue item renew (id={item.Id}, queue={this.options.Name})");

            var addItem = item as QueueItem<TData>;
            this.dequeued.AddOrUpdate(item.Id, addItem, (key, value) =>
            {
                if(item != null)
                {
                    value.RenewedDate = addItem.RenewedDate;
                }

                return value;
            });

            this.logger.LogJournal(LogKeys.Queueing, $"item lock renewed (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogEventPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override Task CompleteAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogDebug($"queue item complete (id={item.Id}, queue={this.options.Name})");

            if(item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            if(!this.dequeued.TryRemove(item.Id, out var dequeuedItem) || dequeuedItem == null)
            {
                throw new Exception($"unable to remove item from the dequeued list, not found (id={item.Id})");
            }

            Interlocked.Increment(ref this.completedCount);
            item.MarkCompleted();

            this.logger.LogJournal(LogKeys.Queueing, $"item completed (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogEventPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override Task AbandonAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogDebug($"queue item abandon (id={item.Id}, queue={this.options.Name})");

            if(item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            if(!this.dequeued.TryRemove(item.Id, out var dequeuedItem) || dequeuedItem == null)
            {
                throw new Exception($"unable to remove item from the dequeued list, not found (id={item.Id})");
            }

            if(dequeuedItem.Attempts < this.options.Retries + 1)
            {
                if(this.options.RetryDelay > TimeSpan.Zero)
                {
                    this.logger.LogDebug($"add item to wait list, for future retry (id={item.Id})");
                    var unawaited = Run.DelayedAsync(
                        this.GetRetryDelay(dequeuedItem.Attempts), () =>
                        {
                            this.queue.Enqueue(dequeuedItem);
                            return Task.CompletedTask;
                        });
                }
                else
                {
                    this.logger.LogDebug($"add item back to queue, for retry (id={item.Id})");
                    var unawaited = Task.Run(() => this.queue.Enqueue(dequeuedItem));
                }
            }
            else
            {
                this.logger.LogDebug($"retry limit exceeded, moving to deadletter (id={item.Id})");
                this.deadletterQueue.Enqueue(dequeuedItem);
            }

            Interlocked.Increment(ref this.abandonedCount);
            item.MarkAbandoned();

            this.logger.LogJournal(LogKeys.Queueing, $"item abandoned (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogEventPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override async Task<QueueMetrics> GetMetricsAsync()
        {
            await this.EnsureQueueAsync().AnyContext();

            return new QueueMetrics
            {
                Queued = this.queue.Count,
                Working = this.dequeued.Count,
                Deadlettered = this.deadletterQueue.Count,
                Enqueued = this.enqueuedCount,
                Dequeued = this.dequeuedCount,
                Completed = this.completedCount,
                Abandoned = this.abandonedCount,
                Errors = this.workerErrorCount,
                Timeouts = 0
            };
        }

        public override async Task ProcessItemsAsync(Func<IQueueItem<TData>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            this.ProcessItems(handler, autoComplete, cancellationToken);
        }

        public override async Task ProcessItemsAsync(bool autoComplete, CancellationToken cancellationToken)
        {
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            if(this.options.Mediator == null)
            {
                throw new NaosException("queue processing error: no mediator instance provided");
            }

            this.ProcessItems(
                async (i, ct) =>
                {
                    using(this.logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogEventPropertyKeys.CorrelationId] = i.Data.As<IHaveCorrelationId>()?.CorrelationId,
                    }))
                    {
                        await this.options.Mediator.Send(new QueueEvent<TData>(i), ct).AnyContext();
                    }
                },
                autoComplete, cancellationToken);
        }

        public override Task DeleteQueueAsync()
        {
            this.queue.Clear();
            this.deadletterQueue.Clear();
            this.dequeued.Clear();
            this.enqueuedCount = 0;
            this.dequeuedCount = 0;
            this.completedCount = 0;
            this.abandonedCount = 0;
            this.workerErrorCount = 0;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.queue.Clear();
            this.deadletterQueue.Clear();
            this.dequeued.Clear();
        }

        protected override Task EnsureQueueAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected override async Task<IQueueItem<TData>> DequeueWithIntervalAsync(CancellationToken cancellationToken)
        {
            await this.EnsureQueueAsync().AnyContext();
            this.logger.LogDebug($"queue item dequeue (queue={this.options.Name}, count={this.queue.Count})");

            if(this.queue.Count == 0)
            {
                this.logger.LogDebug($"no queue items, waiting (name={this.options.Name})");

                while(this.queue.Count == 0 && !cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(this.options.DequeueInterval.Milliseconds).Wait();
                }
            }

            if(this.queue.Count == 0)
            {
                return null;
            }

            if(!this.queue.TryDequeue(out var dequeuedItem) || dequeuedItem == null)
            {
                return null;
            }

            dequeuedItem.Attempts++;
            dequeuedItem.DequeuedDate = DateTime.UtcNow;

            Interlocked.Increment(ref this.dequeuedCount);
            var item = new QueueItem<TData>(dequeuedItem.Id, dequeuedItem.Data.Clone(), this, dequeuedItem.EnqueuedDate, dequeuedItem.Attempts); // clone item

            using(this.logger.BeginScope(new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = item.Data.As<IHaveCorrelationId>()?.CorrelationId,
            }))
            {
                await item.RenewLockAsync();

                this.logger.LogJournal(LogKeys.Queueing, $"item dequeued (id={item.Id}, queue={this.options.Name}, type={typeof(TData).PrettyName()})", LogEventPropertyKeys.TrackEnqueue);
                this.logger.LogTraceEvent(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceEventNames.Queue, DateTime.UtcNow - item.EnqueuedDate);
                this.LastDequeuedDate = DateTime.UtcNow;
                return item;
            }
        }

        private void ProcessItems(Func<IQueueItem<TData>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            var linkedCancellationToken = this.CreateLinkedTokenSource(cancellationToken);

            Task.Run(async () =>
            {
                this.logger.LogInformation($"{{LogKey:l}} processing started (queue={this.options.Name}, type={this.GetType().PrettyName()})", args: new[] { LogKeys.Queueing });
                while(!linkedCancellationToken.IsCancellationRequested)
                {
                    IQueueItem<TData> item = null;
                    try
                    {
                        item = await this.DequeueWithIntervalAsync(linkedCancellationToken.Token).AnyContext();
                    }
                    catch(Exception ex)
                    {
                        this.logger.LogError(ex, $"{{LogKey:l}} processing error: {ex.Message}", args: new[] { LogKeys.Queueing });
                    }

                    if(linkedCancellationToken.IsCancellationRequested || item == null)
                    {
                        await Task.Delay(this.options.ProcessTimeout.Milliseconds);
                        continue;
                    }

                    using(this.logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogEventPropertyKeys.CorrelationId] = item.Data.As<IHaveCorrelationId>()?.CorrelationId,
                    }))
                    {
                        try
                        {
                            await handler(item, linkedCancellationToken.Token).AnyContext();
                            if(autoComplete && !item.IsAbandoned && !item.IsCompleted)
                            {
                                await item.CompleteAsync().AnyContext();
                            }
                        }
                        catch(Exception ex)
                        {
                            Interlocked.Increment(ref this.workerErrorCount);
                            this.logger.LogError(ex, $"{{LogKey:l}} processing error: {ex.Message}", args: new[] { LogKeys.Queueing });

                            if(!item.IsAbandoned && !item.IsCompleted)
                            {
                                await item.AbandonAsync().AnyContext();
                            }
                        }
                    }
                }

                this.logger.LogDebug($"queue processing exiting (name={this.options.Name}, cancellation={linkedCancellationToken.IsCancellationRequested})");
            }, linkedCancellationToken.Token).ContinueWith(t => linkedCancellationToken.Dispose());
        }

        private TimeSpan GetRetryDelay(int attempts)
        {
            var maxMultiplier = this.options.RetryMultipliers.Length > 0 ? this.options.RetryMultipliers.Last() : 1;
            var multiplier = attempts <= this.options.RetryMultipliers.Length ? this.options.RetryMultipliers[attempts - 1] : maxMultiplier;
            return TimeSpan.FromMilliseconds((int)(this.options.RetryDelay.TotalMilliseconds * multiplier));
        }
    }
}

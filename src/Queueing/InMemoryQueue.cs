namespace Naos.Core.Queueing
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.Queueing.Domain;

    public class InMemoryQueue<T> : QueueBase<T, InMemoryQueueOptions>
        where T : class
    {
        private readonly ConcurrentQueue<QueueItem<T>> queue = new ConcurrentQueue<QueueItem<T>>();
        private readonly ConcurrentDictionary<string, QueueItem<T>> dequeued = new ConcurrentDictionary<string, QueueItem<T>>();
        private readonly ConcurrentQueue<QueueItem<T>> deadletterQueue = new ConcurrentQueue<QueueItem<T>>();

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

        public InMemoryQueue(Builder<InMemoryQueueOptionsBuilder, InMemoryQueueOptions> config)
            : this(config(new InMemoryQueueOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public override async Task<string> EnqueueAsync(T data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            await this.EnsureQueueAsync().AnyContext();

            string id = Guid.NewGuid().ToString("N");
            this.logger.LogInformation($"queue item enqueue (id={id}, queue={this.options.Name})");

            var item = new QueueItem<T>(id, data.Clone(), this, DateTime.UtcNow, 0);
            this.queue.Enqueue(item);

            Interlocked.Increment(ref this.enqueuedCount);

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item enqueued (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastEnqueuedDate = DateTime.UtcNow;
            return item.Id;
        }

        public override Task RenewLockAsync(IQueueItem<T> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogInformation($"queue item renew (id={item.Id}, queue={this.options.Name})");

            var addItem = item as QueueItem<T>;
            this.dequeued.AddOrUpdate(item.Id, addItem, (key, value) =>
            {
                if (item != null)
                {
                    value.RenewedDate = addItem.RenewedDate;
                }

                return value;
            });

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item lock renewed (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override Task CompleteAsync(IQueueItem<T> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogInformation($"queue item complete (id={item.Id}, queue={this.options.Name})");

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            if (!this.dequeued.TryRemove(item.Id, out var dequeuedItem) || dequeuedItem == null)
            {
                throw new Exception($"unable to remove item from the dequeued list, not found (id={item.Id})");
            }

            Interlocked.Increment(ref this.completedCount);
            item.MarkCompleted();

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item completed (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override Task AbandonAsync(IQueueItem<T> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogInformation($"queue item abandon (id={item.Id}, queue={this.options.Name})");

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            if (!this.dequeued.TryRemove(item.Id, out var dequeuedItem) || dequeuedItem == null)
            {
                throw new Exception($"unable to remove item from the dequeued list, not found (id={item.Id})");
            }

            if (dequeuedItem.Attempts < this.options.Retries + 1)
            {
                if (this.options.RetryDelay > TimeSpan.Zero)
                {
                    this.logger.LogInformation($"add item to wait list, for future retry (id={item.Id})");
                    var unawaited = Run.DelayedAsync(
                        this.GetRetryDelay(dequeuedItem.Attempts), () =>
                        {
                            this.queue.Enqueue(dequeuedItem);
                            return Task.CompletedTask;
                        });
                }
                else
                {
                    this.logger.LogInformation($"add item back to queue, for retry (id={item.Id})");
                    var unawaited = Task.Run(() => this.queue.Enqueue(dequeuedItem));
                }
            }
            else
            {
                this.logger.LogInformation($"retry limit exceeded, moving to deadletter (id={item.Id})");
                this.deadletterQueue.Enqueue(dequeuedItem);
            }

            Interlocked.Increment(ref this.abandonedCount);
            item.MarkAbandoned();

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item abandoned (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
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
                Deadletter = this.deadletterQueue.Count,
                Enqueued = this.enqueuedCount,
                Dequeued = this.dequeuedCount,
                Completed = this.completedCount,
                Abandoned = this.abandonedCount,
                Errors = this.workerErrorCount,
                Timeouts = 0
            };
        }

        public override async Task ProcessItemsAsync(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            this.ProcessItems(handler, autoComplete, cancellationToken);
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

        protected override async Task<IQueueItem<T>> DequeueWithIntervalAsync(CancellationToken cancellationToken)
        {
            await this.EnsureQueueAsync().AnyContext();
            this.logger.LogInformation($"queue item dequeue (queue={this.options.Name}, count={this.queue.Count})");

            if (this.queue.Count == 0)
            {
                this.logger.LogDebug($"no queue items, waiting (name={this.options.Name})");

                while (this.queue.Count == 0 && !cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(this.options.DequeueInterval.Milliseconds).Wait();
                }
            }

            if (this.queue.Count == 0)
            {
                return null;
            }

            if (!this.queue.TryDequeue(out var dequeuedItem) || dequeuedItem == null)
            {
                return null;
            }

            dequeuedItem.Attempts++;
            dequeuedItem.DequeuedDate = DateTime.UtcNow;

            Interlocked.Increment(ref this.dequeuedCount);
            var item = new QueueItem<T>(dequeuedItem.Id, dequeuedItem.Value.Clone(), this, dequeuedItem.EnqueuedDate, dequeuedItem.Attempts); // clone item
            await item.RenewLockAsync();

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item dequeued (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
            return item;
        }

        private void ProcessItems(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            var linkedCancellationToken = this.CreateLinkedTokenSource(cancellationToken);

            Task.Run(async () =>
            {
                this.logger.LogInformation($"queue processing started (queue={this.options.Name})");

                while (!linkedCancellationToken.IsCancellationRequested)
                {
                    IQueueItem<T> item = null;
                    try
                    {
                        item = await this.DequeueWithIntervalAsync(linkedCancellationToken.Token).AnyContext();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"queue processing error: {ex.Message}");
                    }

                    if (linkedCancellationToken.IsCancellationRequested || item == null)
                    {
                        await Task.Delay(this.options.ProcessTimeout.Milliseconds);
                        continue;
                    }

                    try
                    {
                        await handler(item, linkedCancellationToken.Token).AnyContext();
                        if (autoComplete && !item.IsAbandoned && !item.IsCompleted)
                        {
                            await item.CompleteAsync().AnyContext();
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref this.workerErrorCount);
                        this.logger.LogError(ex, $"queue processing error: {ex.Message}");

                        if (!item.IsAbandoned && !item.IsCompleted)
                        {
                            await item.AbandonAsync().AnyContext();
                        }
                    }
                }

                this.logger.LogInformation($"queue processing exiting (name={this.options.Name}, cancellation={linkedCancellationToken.IsCancellationRequested})");
            }, linkedCancellationToken.Token).ContinueWith(t => linkedCancellationToken.Dispose());
        }

        private TimeSpan GetRetryDelay(int attempts)
        {
            int maxMultiplier = this.options.RetryMultipliers.Length > 0 ? this.options.RetryMultipliers.Last() : 1;
            int multiplier = attempts <= this.options.RetryMultipliers.Length ? this.options.RetryMultipliers[attempts - 1] : maxMultiplier;
            return TimeSpan.FromMilliseconds((int)(this.options.RetryDelay.TotalMilliseconds * multiplier));
        }
    }
}

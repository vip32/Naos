namespace Naos.Queueing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Queueing.Domain;
    using Naos.Tracing.Domain;

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
        private bool isProcessing;

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

            var correlationId = data.As<IHaveCorrelationId>()?.CorrelationId ?? IdGenerator.Instance.Next;
            using (this.Logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = correlationId
            }))
            using (var scope = this.Options.Tracer?.BuildSpan($"enqueue {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Producer).Activate(this.Logger))
            {
                await this.EnsureQueueAsync().AnyContext();

                var item = new QueueItem<TData>(IdGenerator.Instance.Next, data/*.Clone()*/, this)
                {
                    CorrelationId = correlationId,
                    TraceId = scope.Span.TraceId,
                    SpanId = scope.Span.SpanId
                };

                this.Logger.LogDebug($"{{LogKey:l}} queue item enqueue (id={item.Id}, queue={this.Options.QueueName}, type={this.GetType().PrettyName()})", LogKeys.Queueing);
                this.queue.Enqueue(item);

                Interlocked.Increment(ref this.enqueuedCount);

                this.Logger.LogJournal(LogKeys.Queueing, $"item enqueued (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackEnqueue);
                this.Logger.LogTrace(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceNames.Queue);
                this.LastEnqueuedDate = DateTime.UtcNow;
                return item.Id;
            }
        }

        public override Task RenewLockAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.Logger.LogDebug($"{{LogKey:l}} queue item renew (id={item.Id}, queue={this.Options.QueueName})", LogKeys.Queueing);

            var addItem = item as QueueItem<TData>;
            this.dequeued.AddOrUpdate(item.Id, addItem, (key, value) =>
            {
                if (item != null)
                {
                    value.RenewedDate = addItem.RenewedDate;
                }

                return value;
            });

            this.Logger.LogJournal(LogKeys.Queueing, $"item lock renewed (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override Task CompleteAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.Logger.LogDebug($"{{LogKey:l}} queue item complete (id={item.Id}, queue={this.Options.QueueName})", LogKeys.Queueing);

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

#pragma warning disable IDE0067 // Dispose objects before losing scope
#pragma warning disable CA2000 // Dispose objects before losing scope
            if (!this.dequeued.TryRemove(item.Id, out var dequeuedItem) || dequeuedItem == null)
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning restore IDE0067 // Dispose objects before losing scope
            {
                throw new Exception($"unable to remove item from the dequeued list, not found (id={item.Id})");
            }

            Interlocked.Increment(ref this.completedCount);
            item.MarkCompleted();

            this.Logger.LogJournal(LogKeys.Queueing, $"item completed (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public override Task AbandonAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.Logger.LogDebug($"{{LogKey:l}} queue item abandon (id={item.Id}, queue={this.Options.QueueName})", LogKeys.Queueing);

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0068 // Use recommended dispose pattern
            if (!this.dequeued.TryRemove(item.Id, out var dequeuedItem) || dequeuedItem == null)
#pragma warning restore IDE0068 // Use recommended dispose pattern
#pragma warning restore CA2000 // Dispose objects before losing scope
            {
                throw new Exception($"unable to remove item from the dequeued list, not found (id={item.Id})");
            }

            if (dequeuedItem.Attempts < this.Options.Retries + 1)
            {
                if (this.Options.RetryDelay > TimeSpan.Zero)
                {
                    this.Logger.LogDebug($"{{LogKey:l}} add item to wait list, for future retry (id={item.Id})", LogKeys.Queueing);
                    var unawaited = Run.DelayedAsync(
                        this.GetRetryDelay(dequeuedItem.Attempts), () =>
                        {
                            this.queue.Enqueue(dequeuedItem);
                            return Task.CompletedTask;
                        });
                }
                else
                {
                    this.Logger.LogDebug($"{{LogKey:l}} add item back to queue, for retry (id={item.Id})", LogKeys.Queueing);
                    var unawaited = Task.Run(() => this.queue.Enqueue(dequeuedItem));
                }
            }
            else
            {
                this.Logger.LogDebug($"retry limit exceeded, moving to deadletter (id={item.Id})");
                this.deadletterQueue.Enqueue(dequeuedItem);
            }

            Interlocked.Increment(ref this.abandonedCount);
            item.MarkAbandoned();

            this.Logger.LogJournal(LogKeys.Queueing, $"item abandoned (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
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

            if (this.Options.Mediator == null)
            {
                throw new NaosException("queue processing error: no mediator instance provided");
            }

            if (!this.isProcessing)
            {
                this.ProcessItems(
                    async (i, ct) =>
                    {
                        using (this.Logger.BeginScope(new Dictionary<string, object>
                        {
                            [LogPropertyKeys.CorrelationId] = i.CorrelationId,
                        }))
                        //using (var scope = this.Options.Tracer?.BuildSpan($"dequeue2 {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Consumer, new Span(i.TraceId, i.SpanId)).Activate(this.Logger))
                        {
                            await this.Options.Mediator.Send(new QueueEvent<TData>(i), ct).AnyContext();
                        }
                    },
                    autoComplete, cancellationToken);
                this.isProcessing = true;
            }
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
            this.Logger.LogDebug($"{{LogKey:l}} queue item dequeue (queue={this.Options.QueueName}, count={this.queue.Count})", LogKeys.Queueing);

            if (this.queue.Count == 0)
            {
                this.Logger.LogDebug($"{{LogKey:l}} no queue items, waiting (name={this.Options.QueueName})", LogKeys.Queueing);

                while (this.queue.Count == 0
                    && !cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(this.Options.DequeueInterval).Wait();
                }
            }

            if (this.queue.Count == 0)
            {
                return null;
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0067 // Dispose objects before losing scope
            if (!this.queue.TryDequeue(out var item) || item == null)
            {
                return null;
            }

            item.Attempts++;
            item.DequeuedDate = DateTime.UtcNow;

            Interlocked.Increment(ref this.dequeuedCount);
            //var item = new QueueItem<TData>(dequeuedItem.Id, dequeuedItem.Data/*.Clone()*/, this, dequeuedItem.EnqueuedDate, dequeuedItem.Attempts); // clone item

            using (this.Logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = item.CorrelationId,
            }))
            {
                await item.RenewLockAsync().AnyContext();

                this.Logger.LogJournal(LogKeys.Queueing, $"item dequeued (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackEnqueue);
                this.Logger.LogTrace(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceNames.Queue, DateTime.UtcNow - item.EnqueuedDate);
                this.LastDequeuedDate = DateTime.UtcNow;
                return item;
            }
        }

        private void ProcessItems(Func<IQueueItem<TData>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var linkedCancellationToken = this.CreateLinkedTokenSource(cancellationToken);
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CA2000 // Dispose objects before losing scope

            Task.Run(async () =>
            {
                this.Logger.LogInformation($"{{LogKey:l}} processing started (queue={this.Options.QueueName}, type={this.GetType().PrettyName()})", args: new[] { LogKeys.Queueing });
                while (!linkedCancellationToken.IsCancellationRequested)
                {
                    IQueueItem<TData> item = null;
                    try
                    {
                        item = await this.DequeueWithIntervalAsync(linkedCancellationToken.Token).AnyContext();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, $"{{LogKey:l}} queue processing error: {ex.GetFullMessage()}", args: new[] { LogKeys.Queueing });
                    }

                    if (linkedCancellationToken.IsCancellationRequested || item == null)
                    {
                        await Task.Delay(this.Options.ProcessInterval, linkedCancellationToken.Token).AnyContext();
                        continue;
                    }

                    using (this.Logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogPropertyKeys.CorrelationId] = item.CorrelationId,
                    }))
                    using (var scope = this.Options.Tracer?.BuildSpan($"dequeue {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Consumer, new Span(item.TraceId, item.SpanId)).Activate(this.Logger))
                    {
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
                            scope.Span.SetStatus(SpanStatus.Failed, ex.GetFullMessage());
                            this.Logger.LogError(ex, $"{{LogKey:l}} queue processing error: {ex.GetFullMessage()}", args: new[] { LogKeys.Queueing });

                            if (!item.IsAbandoned && !item.IsCompleted)
                            {
                                await item.AbandonAsync().AnyContext();
                            }
                        }
                    }
                }

                this.Logger.LogDebug($"{{LogKey:l}} queue processing exiting (name={this.Options.QueueName}, cancellation={linkedCancellationToken.IsCancellationRequested})", LogKeys.Queueing);
            }, linkedCancellationToken.Token).ContinueWith(t => linkedCancellationToken.Dispose());
        }

        private TimeSpan GetRetryDelay(int attempts)
        {
            var maxMultiplier = this.Options.RetryMultipliers.Length > 0 ? this.Options.RetryMultipliers.Last() : 1;
            var multiplier = attempts <= this.Options.RetryMultipliers.Length ? this.Options.RetryMultipliers[attempts - 1] : maxMultiplier;
            return TimeSpan.FromMilliseconds((int)(this.Options.RetryDelay.TotalMilliseconds * multiplier));
        }
    }
}

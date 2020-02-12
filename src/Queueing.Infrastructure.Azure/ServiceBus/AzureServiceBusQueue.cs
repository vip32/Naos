namespace Naos.Queueing.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Queueing.Domain;
    using Naos.Tracing.Domain;

    public class AzureServiceBusQueue<TData> : QueueBase<TData, AzureServiceBusQueueOptions>
         where TData : class
    {
        private readonly ManagementClient managementClient;
        private MessageSender queueSender;
        private MessageReceiver queueReceiver;
        private long enqueuedCount;
        private long dequeuedCount;
        private long completedCount;
        private long abandonedCount;
        private long workerErrorCount;

        public AzureServiceBusQueue()
            : this(o => o)
        {
        }

        public AzureServiceBusQueue(AzureServiceBusQueueOptions options)
            : base(options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));
            EnsureArg.IsNotNullOrEmpty(options.QueueName, nameof(options.QueueName));

            this.managementClient = new ManagementClient(options.ConnectionString);
        }

        public AzureServiceBusQueue(Builder<AzureServiceBusQueueOptionsBuilder, AzureServiceBusQueueOptions> optionsBuilder)
            : this(optionsBuilder(new AzureServiceBusQueueOptionsBuilder()).Build())
        {
        }

        private bool QueueIsCreated => this.queueReceiver != null && this.queueSender != null;

        public override async Task<string> EnqueueAsync(TData data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            this.EnsureMetaData(data);

            var correlationId = data.As<IHaveCorrelationId>()?.CorrelationId ?? IdGenerator.Instance.Next;
            using (this.Logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = correlationId,
            }))
            using (var scope = this.Options.Tracer?.BuildSpan($"enqueue {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Producer).Activate(this.Logger))
            {
                await this.EnsureQueueAsync().AnyContext();

                var id = IdGenerator.Instance.Next;
                this.Logger.LogDebug($"{{LogKey:l}} queue item enqueue (id={id}, queue={this.Options.QueueName}, type={this.GetType().PrettyName()})", LogKeys.Queueing);

                Interlocked.Increment(ref this.enqueuedCount);
                using (var stream = new MemoryStream())
                {
                    this.Serializer.Serialize(data, stream);
                    var message = new Message(stream.ToArray())
                    {
                        MessageId = id,
                        CorrelationId = correlationId,
                    };
                    //message.UserProperties.AddOrUpdate("CorrelationId", scope.Span.TraceId);
                    message.UserProperties.AddOrUpdate("TraceId", scope?.Span?.TraceId);
                    message.UserProperties.AddOrUpdate("SpanId", scope?.Span?.SpanId);

                    await this.queueSender.SendAsync(message).AnyContext();

                    this.Logger.LogJournal(LogKeys.Queueing, $"queue item enqueued (id={message.MessageId}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackEnqueue);
                    this.Logger.LogTrace(LogKeys.Queueing, message.MessageId, typeof(TData).PrettyName(), LogTraceNames.Queue);
                    this.LastEnqueuedDate = DateTime.UtcNow;
                    return message.MessageId;
                }
            }
        }

        public override async Task<IQueueItem<TData>> DequeueAsync(TimeSpan? timeout = null)
        {
            await this.EnsureQueueAsync().AnyContext();
            this.Logger.LogDebug($"{{LogKey:l}} queue item dequeue (queue={this.Options.QueueName})", LogKeys.Queueing);

            // TODO: ReceiveBatchAsync?
            Message message;
            if (!timeout.HasValue || timeout.Value.Ticks == 0)
            {
                if ((await this.GetMetricsAsync().AnyContext()).Queued == 0)
                {
                    return default; // skip the wait if no messages (servicebus)
                }

                message = await this.queueReceiver.ReceiveAsync(TimeSpan.FromSeconds(5)).AnyContext();
            }
            else
            {
                message = await this.queueReceiver.ReceiveAsync(timeout.Value).AnyContext();
            }

            return this.HandleDequeue(message);
        }

        public override async Task RenewLockAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.Logger.LogDebug($"{{LogKey:l}} queue item renew (id={item.Id}, queue={this.Options.QueueName})", LogKeys.Queueing);

            await this.queueReceiver.RenewLockAsync(item.Id).AnyContext();

            this.Logger.LogJournal(LogKeys.Queueing, $"item lock renewed (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task CompleteAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            await this.queueReceiver.CompleteAsync(item.Id).AnyContext();
            Interlocked.Increment(ref this.completedCount);
            item.MarkCompleted();

            this.Logger.LogJournal(LogKeys.Queueing, $"queue item completed (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task AbandonAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.Logger.LogDebug($"{{LogKey:l}} queue item abandon (id={item.Id}, queue={this.Options.QueueName})", LogKeys.Queueing);

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            await this.queueReceiver.AbandonAsync(item.Id).AnyContext();
            Interlocked.Increment(ref this.abandonedCount);
            item.MarkAbandoned();

            this.Logger.LogJournal(LogKeys.Queueing, $"item abandoned (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task<QueueMetrics> GetMetricsAsync()
        {
            await this.EnsureQueueAsync().AnyContext();

            var info = await this.managementClient.GetQueueRuntimeInfoAsync(this.Options.QueueName).AnyContext();
            return new QueueMetrics
            {
                Queued = info.MessageCount,
                Working = 0,
                Deadlettered = info.MessageCountDetails.DeadLetterMessageCount,
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

            this.Logger.LogInformation($"{{LogKey:l}} processing started (queue={this.Options.QueueName}, type={this.GetType().PrettyName()})", args: new[] { LogKeys.Queueing });
            this.queueReceiver.RegisterMessageHandler(async (msg, ct) =>
            {
                var item = this.HandleDequeue(msg);

                using (this.Logger.BeginScope(new Dictionary<string, object>
                {
                    [LogPropertyKeys.CorrelationId] = item.CorrelationId
                }))
                using (var scope = this.Options.Tracer?.BuildSpan($"dequeue {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Consumer, new Span(item.TraceId, item.SpanId)).Activate(this.Logger))
                {
                    try
                    {
                        using (var linkedCancellationToken = this.CreateLinkedTokenSource(cancellationToken))
                        {
                            await handler(item, linkedCancellationToken.Token).AnyContext();
                        }

                        if (autoComplete && !item.IsAbandoned && !item.IsCompleted)
                        {
                            await item.CompleteAsync().AnyContext();
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref this.workerErrorCount);
                        scope.Span.SetStatus(SpanStatus.Failed, ex.GetFullMessage());
                        this.Logger.LogError(ex, $"{{LogKey:l}} queue processing failed: {ex.GetFullMessage()}", args: new[] { LogKeys.Queueing });

                        if (!item.IsAbandoned && !item.IsCompleted)
                        {
                            await item.AbandonAsync().AnyContext();
                        }
                    }
                }
            }, new MessageHandlerOptions(this.ExceptionReceivedHandler));
        }

        public override async Task ProcessItemsAsync(bool autoComplete, CancellationToken cancellationToken)
        {
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            if (this.Options.Mediator == null)
            {
                throw new NaosException("queue processing failed: no mediator instance provided");
            }

            await this.ProcessItemsAsync(
                async (i, ct) =>
                {
                    using (this.Logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogPropertyKeys.CorrelationId] = i.CorrelationId,
                    }))
                    //using (var scope = this.Options.Tracer?.BuildSpan($"dequeue {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Consumer, new Span(i.TraceId, i.SpanId)).Activate(this.Logger))
                    {
                        await this.Options.Mediator.Send(new QueueEvent<TData>(i), ct).AnyContext();
                    }
                },
                autoComplete, cancellationToken).AnyContext();
        }

        public override async Task DeleteQueueAsync()
        {
            if (await this.managementClient.QueueExistsAsync(this.Options.QueueName).AnyContext())
            {
                await this.managementClient.DeleteQueueAsync(this.Options.QueueName).AnyContext();
            }

            this.queueSender = null;
            this.queueReceiver = null;
            this.enqueuedCount = 0;
            this.dequeuedCount = 0;
            this.completedCount = 0;
            this.abandonedCount = 0;
            this.workerErrorCount = 0;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.queueSender?.CloseAsync();
            this.queueSender = null;
            this.queueReceiver?.CloseAsync();
            this.queueReceiver = null;
            this.managementClient.CloseAsync();
        }

        protected override async Task EnsureQueueAsync(CancellationToken cancellationToken = default)
        {
            if (this.QueueIsCreated)
            {
                return;
            }

            try
            {
                await this.managementClient.CreateQueueAsync(this.Options.AsQueueDescription()).AnyContext();
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // log?
            }

            this.queueSender = new MessageSender(
                this.Options.ConnectionString,
                this.Options.QueueName,
                this.Options.RetryPolicy);

            this.queueReceiver = new MessageReceiver(
                this.Options.ConnectionString,
                this.Options.QueueName,
                ReceiveMode.PeekLock, // = slow, fast = ReceiveAndDelete?
                this.Options.Retries == 0 ? new NoRetry() : this.Options.RetryPolicy);
        }

        protected override Task<IQueueItem<TData>> DequeueWithIntervalAsync(CancellationToken cancellationToken)
        {
            // azure ServiceBus does not support CancellationTokens > use TimeSpan overload instead.default 30 second timeout
            return this.DequeueAsync();
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs e)
        {
            if (!(e.Exception is MessageLockLostException))
            {
                this.Logger.LogError(e.Exception, $"{{LogKey:l}} processing error:  {e.ExceptionReceivedContext.EntityPath} {e.Exception.Message}", args: new[] { LogKeys.Queueing });
            }

            return Task.CompletedTask;
        }

        private IQueueItem<TData> HandleDequeue(Message message)
        {
            if (message == null)
            {
                return null;
            }

            Interlocked.Increment(ref this.dequeuedCount);

            var item = new QueueItem<TData>(
                message.SystemProperties.LockToken,
                this.Serializer.Deserialize<TData>(message.Body),
                this,
                message.SystemProperties.EnqueuedTimeUtc,
                message.SystemProperties.DeliveryCount)
            {
                CorrelationId = message.CorrelationId, //message.UserProperties.TryGetValue("CorrelationId")?.ToString(),
                TraceId = message.UserProperties.TryGetValue("TraceId")?.ToString(),
                SpanId = message.UserProperties.TryGetValue("SpanId")?.ToString()
            };

            using (this.Logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = item.Data.As<IHaveCorrelationId>()?.CorrelationId,
            }))
            {
                this.Logger.LogJournal(LogKeys.Queueing, $"item dequeued (id={item.Id}, queue={this.Options.QueueName}, data={typeof(TData).PrettyName()})", LogPropertyKeys.TrackDequeue);
                this.Logger.LogTrace(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceNames.Queue, DateTime.UtcNow - item.EnqueuedDate);
                this.LastDequeuedDate = DateTime.UtcNow;

                return item;
            }
        }
    }
}

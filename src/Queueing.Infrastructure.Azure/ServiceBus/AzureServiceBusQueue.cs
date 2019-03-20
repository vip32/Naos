namespace Naos.Core.Queueing.Infrastructure.Azure
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.Queueing.Domain;

    public class AzureServiceBusQueue<T> : BaseQueue<T, AzureServiceBusQueueOptions>
         where T : class
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
            EnsureArg.IsNotNullOrEmpty(options.Name, nameof(options.Name));

            this.managementClient = new ManagementClient(options.ConnectionString);
        }

        public AzureServiceBusQueue(Builder<AzureServiceBusQueueOptionsBuilder, AzureServiceBusQueueOptions> optionsBuilder)
            : this(optionsBuilder(new AzureServiceBusQueueOptionsBuilder()).Build())
        {
        }

        private bool QueueIsCreated => this.queueReceiver != null && this.queueSender != null;

        public override async Task<string> EnqueueAsync(T data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            await this.EnsureQueueAsync().AnyContext();

            string id = RandomGenerator.GenerateString(13, true);
            this.logger.LogInformation($"queue item enqueue (id={id}, queue={this.options.Name})");

            Interlocked.Increment(ref this.enqueuedCount);
            var stream = new MemoryStream();
            this.serializer.Serialize(data, stream);
            var brokeredMessage = new Message(stream.ToArray())
            {
                // TODO: store correlationid?
                MessageId = id
            };
            await this.queueSender.SendAsync(brokeredMessage).AnyContext();

            var item = new QueueItem<T>(brokeredMessage.MessageId, data, this, DateTime.UtcNow, 0);

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item enqueued (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastEnqueuedDate = DateTime.UtcNow;
            return brokeredMessage.MessageId;
        }

        public override async Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null)
        {
            await this.EnsureQueueAsync().AnyContext();
            this.logger.LogInformation($"queue item dequeue (queue={this.options.Name})");

            // TODO: ReceiveBatchAsync?
            Message message;
            if(!timeout.HasValue || timeout.Value.Ticks == 0)
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

        public override async Task RenewLockAsync(IQueueItem<T> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogInformation($"queue item renew (id={item.Id}, queue={this.options.Name})");

            await this.queueReceiver.RenewLockAsync(item.Id).AnyContext();

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item lock renewed (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task CompleteAsync(IQueueItem<T> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogInformation($"queue item complete (id={item.Id}, queue={this.options.Name})");

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            await this.queueReceiver.CompleteAsync(item.Id).AnyContext();
            Interlocked.Increment(ref this.completedCount);
            item.MarkCompleted();

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item completed (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task AbandonAsync(IQueueItem<T> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.logger.LogInformation($"queue item abandon (id={item.Id}, queue={this.options.Name})");

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            await this.queueReceiver.AbandonAsync(item.Id).AnyContext();
            Interlocked.Increment(ref this.abandonedCount);
            item.MarkAbandoned();

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item abandoned (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;
        }

        public override async Task<QueueMetrics> GetMetricsAsync()
        {
            await this.EnsureQueueAsync().AnyContext();

            var info = await this.managementClient.GetQueueRuntimeInfoAsync(this.options.Name).AnyContext();
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

        public override async Task ProcessItemsAsync(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(handler, nameof(handler));
            await this.EnsureQueueAsync(cancellationToken).AnyContext();

            this.logger.LogInformation($"queue processing started (queue={this.options.Name})");
            this.queueReceiver.RegisterMessageHandler(async (msg, ct) =>
            {
                var item = this.HandleDequeue(msg);

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
                    this.logger.LogError(ex, $"queue processing error: {ex.Message}");

                    if (!item.IsAbandoned && !item.IsCompleted)
                    {
                        await item.AbandonAsync().AnyContext();
                    }
                }
            }, new MessageHandlerOptions(this.ExceptionReceivedHandler));
        }

        public override async Task DeleteQueueAsync()
        {
            if (await this.managementClient.QueueExistsAsync(this.options.Name).AnyContext())
            {
                await this.managementClient.DeleteQueueAsync(this.options.Name).AnyContext();
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
                await this.managementClient.CreateQueueAsync(this.options.AsQueueDescription()).AnyContext();
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // log?
            }

            this.queueSender = new MessageSender(
                this.options.ConnectionString,
                this.options.Name,
                this.options.RetryPolicy);

            this.queueReceiver = new MessageReceiver(
                this.options.ConnectionString,
                this.options.Name,
                ReceiveMode.PeekLock, // = slow, fast = ReceiveAndDelete?
                this.options.RetryPolicy);
        }

        protected override Task<IQueueItem<T>> DequeueWithIntervalAsync(CancellationToken cancellationToken)
        {
            // azure ServiceBus does not support CancellationTokens > use TimeSpan overload instead.default 30 second timeout
            return this.DequeueAsync();
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs e)
        {
            this.logger.LogWarning($"queue processing error: {e.ExceptionReceivedContext.EntityPath} {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private IQueueItem<T> HandleDequeue(Message message)
        {
            if (message == null)
            {
                return null;
            }

            Interlocked.Increment(ref this.dequeuedCount);

            var item = new QueueItem<T>(
                message.SystemProperties.LockToken,
                this.serializer.Deserialize<T>(message.Body),
                this,
                message.SystemProperties.EnqueuedTimeUtc,
                message.SystemProperties.DeliveryCount);

            this.logger.LogJournal(LogEventPropertyKeys.TrackEnqueue, $"{{LogKey:l}} item dequeued (id={item.Id}, queue={this.options.Name}, type={typeof(T).PrettyName()})", args: new[] { LogEventKeys.Queueing });
            this.LastDequeuedDate = DateTime.UtcNow;

            return item;
        }
    }
}

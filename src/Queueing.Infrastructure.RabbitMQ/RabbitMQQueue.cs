namespace Naos.Queueing.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Queueing.Domain;
    using Naos.Tracing.Domain;
    using Polly;

    public class RabbitMQQueue<TData> : QueueBase<TData, RabbitMQQueueOptions>
         where TData : class
    {
        private IModel channel;
        private bool consumeStarted;
        private long enqueuedCount;
        private long dequeuedCount;
        private long completedCount;
        private long abandonedCount;
        private long workerErrorCount;
        private AsyncEventingBasicConsumer consumer;

        public RabbitMQQueue()
            : this(o => o)
        {
        }

        public RabbitMQQueue(RabbitMQQueueOptions options)
            : base(options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));
            EnsureArg.IsNotNullOrEmpty(options.ExchangeName, nameof(options.ExchangeName));
            EnsureArg.IsNotNullOrEmpty(options.QueueName, nameof(options.QueueName));

            this.channel = this.CreateChannel(options.QueueName);
        }

        public RabbitMQQueue(Builder<RabbitMQQueueOptionsBuilder, RabbitMQQueueOptions> optionsBuilder)
            : this(optionsBuilder(new RabbitMQQueueOptionsBuilder()).Build())
        {
        }

        public override Task<string> EnqueueAsync(TData data)
        {
            EnsureArg.IsNotNull(data, nameof(data));
            this.EnsureMetaData(data);

            var correlationId = data.As<IHaveCorrelationId>()?.CorrelationId ?? IdGenerator.Instance.Next;
            var messageName = data.GetType().PrettyName();
            var routingKey = this.GetRoutingKey(messageName);

            using (this.Logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = correlationId,
            }))
            using (var scope = this.Options.Tracer?.BuildSpan($"enqueue {this.Options.QueueName}", LogKeys.Queueing, SpanKind.Producer).Activate(this.Logger))
            {
                //await this.EnsureQueueAsync().AnyContext();

                var id = IdGenerator.Instance.Next;
                this.Logger.LogDebug($"{{LogKey:l}} queue item enqueue (id={id}, queue={this.Options.QueueName}, type={this.GetType().PrettyName()})", LogKeys.Queueing);

                Interlocked.Increment(ref this.enqueuedCount);
                var policy = Policy.Handle<BrokerUnreachableException>()
                    .Or<SocketException>()
                    .WaitAndRetry(this.Options.Retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        this.Logger.LogWarning(ex, "{LogKey:l} could not enqueue item: {MessageId} after {Timeout}s ({ExceptionMessage})", LogKeys.AppMessaging, id, $"{time.TotalSeconds:n1}", ex.Message);
                    });

                using (var channel = this.Options.Provider.CreateModel())
                {
                    var rabbitMQMessage = this.Serializer.SerializeToBytes(data);

                    channel.ExchangeDeclare(exchange: this.Options.ExchangeName, type: "direct");
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent
                    properties.Persistent = true;
                    //properties.AppId = message.Origin; // TODO: get from descriptor
                    properties.Type = messageName;
                    properties.MessageId = id;
                    properties.CorrelationId = correlationId;
                    if(this.Options.Expiration.HasValue && this.Options.Expiration.Value.Milliseconds > 0)
                    {
                        properties.Expiration = this.Options.Expiration.Value.Milliseconds.ToString(); // https://www.rabbitmq.com/ttl.html
                    }

                    //properties.Headers.Add("EnqueuedDate", DateTime.UtcNow);
                    if (scope?.Span != null)
                    {
                        properties.Headers ??= new Dictionary<string, object>();
                        // propagate the span infos
                        properties.Headers.Add("TraceId", scope.Span.TraceId);
                        properties.Headers.Add("SpanId", scope.Span.SpanId);
                    }

                    policy.Execute(() =>
                    {
                        channel.BasicPublish(
                            exchange: this.Options.ExchangeName,
                            routingKey: routingKey,
                            mandatory: true,
                            basicProperties: properties,
                            body: rabbitMQMessage);
                    });
                }

                this.Logger.LogJournal(LogKeys.Queueing, $"queue item enqueued: {typeof(TData).PrettyName()} (id={id}, queue={this.Options.QueueName})", LogPropertyKeys.TrackEnqueue);
                this.Logger.LogTrace(LogKeys.Queueing, id, typeof(TData).PrettyName(), LogTraceNames.Queue);
                this.LastEnqueuedDate = DateTime.UtcNow;

                return Task.FromResult(id);
            }
        }

        public override async Task<IQueueItem<TData>> DequeueAsync(TimeSpan? timeout = null)
        {
            await this.EnsureQueueAsync().AnyContext();
            this.Logger.LogDebug($"{{LogKey:l}} queue item dequeue (queue={this.Options.QueueName})", LogKeys.Queueing);

            BasicGetResult result;
            if (!timeout.HasValue || timeout.Value.Ticks == 0)
            {
                //if ((await this.GetMetricsAsync().AnyContext()).Queued == 0)
                //{
                //    return default; // skip the wait if no messages (servicebus)
                //}

                result = this.channel.BasicGet(this.Options.QueueName, true);
                //message = await this.queueReceiver.ReceiveAsync(TimeSpan.FromSeconds(5)).AnyContext();
            }
            else
            {
                //message = await this.queueReceiver.ReceiveAsync(timeout.Value).AnyContext();
                // TODO: wait for timeout
                result = this.channel.BasicGet(this.Options.QueueName, true);
            }

            return this.HandleDequeue(result.BasicProperties, result.Body, result.DeliveryTag);
        }

        public override Task RenewLockAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));
            this.Logger.LogDebug($"{{LogKey:l}} queue item renew (id={item.Id}, queue={this.Options.QueueName})", LogKeys.Queueing);

            //await this.queueReceiver.RenewLockAsync(item.Id).AnyContext();

            this.Logger.LogJournal(LogKeys.Queueing, $"queue item lock renewed: {typeof(TData).PrettyName()} (id={item.Id}, queue={this.Options.QueueName})", LogPropertyKeys.TrackDequeue);
            this.LastDequeuedDate = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        public override Task CompleteAsync(IQueueItem<TData> item)
        {
            EnsureArg.IsNotNull(item, nameof(item));
            EnsureArg.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            if (item.IsAbandoned || item.IsCompleted)
            {
                throw new InvalidOperationException($"queue item has already been completed or abandoned (id={item.Id})");
            }

            var tag = item.Properties.GetValueOrDefault<ulong>("DeliveryTag");
            if(tag > 0)
            {
                this.channel.BasicAck(tag, false);
                Interlocked.Increment(ref this.completedCount);
                item.MarkCompleted();

                this.Logger.LogJournal(LogKeys.Queueing, $"queue item completed: {typeof(TData).PrettyName()} (id={item.Id}, queue={this.Options.QueueName})", LogPropertyKeys.TrackDequeue);
                this.LastDequeuedDate = DateTime.UtcNow;
            }

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

            var tag = item.Properties.GetValueOrDefault<ulong>("DeliveryTag");
            if (tag > 0)
            {
                this.channel.BasicNack(tag, false, false);
                Interlocked.Increment(ref this.abandonedCount);
                item.MarkAbandoned();

                this.Logger.LogJournal(LogKeys.Queueing, $"queue item abandoned: {typeof(TData).PrettyName()} (id={item.Id}, queue={this.Options.QueueName})", LogPropertyKeys.TrackDequeue);
                this.LastDequeuedDate = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        public override async Task<QueueMetrics> GetMetricsAsync()
        {
            await this.EnsureQueueAsync().AnyContext();

            //var info = await this.managementClient.GetQueueRuntimeInfoAsync(this.Options.QueueName).AnyContext();

            return new QueueMetrics
            {
                Queued = this.channel.MessageCount(this.Options.QueueName),
                Working = 0,
                //Deadlettered = info.MessageCountDetails.DeadLetterMessageCount,
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
            this.StartBasicConsume(this.Options.QueueName);

            this.consumer.Received += async (model, args) =>
            {
                var messageName = args.RoutingKey;
                this.Logger.LogDebug($"{{LogKey:l}} processing rabbitmq message (name={messageName}, id={args.BasicProperties.MessageId})", LogKeys.Queueing);
                try
                {
                    var item = this.HandleDequeue(args.BasicProperties, args.Body, args.DeliveryTag);

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
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"{{LogKey:l}} error processing rabbitmq message (name={messageName}, id={args.BasicProperties.MessageId})", LogKeys.Queueing);
                }
            };
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

        public override Task DeleteQueueAsync()
        {
            this.channel.QueueDelete(this.Options.QueueName);

            this.consumer = null;
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
            this.channel?.Dispose();
        }

        protected override Task<IQueueItem<TData>> DequeueWithIntervalAsync(CancellationToken cancellationToken)
        {
            // azure ServiceBus does not support CancellationTokens > use TimeSpan overload instead.default 30 second timeout
            return this.DequeueAsync();
        }

        protected override Task EnsureQueueAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private IModel CreateChannel(string queueName)
        {
            if (!this.Options.Provider.IsConnected)
            {
                this.Options.Provider.TryConnect();
            }

            this.Logger.LogInformation($"{{LogKey:l}} declare rabbitmq consumer channel (exchange={this.Options.ExchangeName}, queue={queueName})", LogKeys.Queueing);

            try
            {
                var channel = this.Options.Provider.CreateModel();
                channel.ExchangeDeclare(exchange: this.Options.ExchangeName, type: "direct"); /*durable: true*/
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null); // TODO: queue expiration (ttl) https://www.rabbitmq.com/ttl.html
                channel.CallbackException += (sender, ea) =>
                {
                    this.Logger.LogWarning($"{{LogKey:l}} recreate rabbitmq consumer channel (queue={queueName})", LogKeys.Queueing);

                    this.channel.Dispose();
                    this.channel = this.CreateChannel(queueName);
                    //this.StartBasicConsume(queueName);
                };

                return channel;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"{{LogKey:l}} rabbitmq channel cannot be created (exchange={this.Options.ExchangeName}, queue={queueName})", LogKeys.Queueing);
                return null;
            }
        }

        private void StartBasicConsume(string queueName)
        {
            if (!this.consumeStarted) // TODO: lock!
            {
                this.Logger.LogInformation($"{{LogKey:l}} start rabbitmq consume (exchange={this.Options.ExchangeName}, queue={queueName})", LogKeys.Queueing);

                if (this.channel != null)
                {
                    if (this.consumer == null)
                    {
                        this.consumer = new AsyncEventingBasicConsumer(this.channel);
                    }

                    var messageName = typeof(TData).PrettyName();
                    var routingKey = this.GetRoutingKey(messageName);

                    this.channel.QueueBind(
                    exchange: this.Options.ExchangeName,
                    queue: queueName,
                    routingKey: routingKey);

                    this.channel.BasicConsume(
                        queue: queueName,
                        autoAck: false,
                        consumer: this.consumer);

                    this.consumeStarted = true;
                }
                else
                {
                    this.Logger.LogError($"{{LogKey:l}} start rabbitmq consume cannot operate on empty channel (exchange={this.Options.ExchangeName}, queue={queueName})", LogKeys.Queueing);
                }
            }
        }

        private string GetRoutingKey(string messageName)
        {
            var ruleName = messageName;

            if (!this.Options.FilterScope.IsNullOrEmpty())
            {
                ruleName += $"-{this.Options.FilterScope}";
            }

            return ruleName; //.Replace("<", "_").Replace(">", "_");
        }

        private IQueueItem<TData> HandleDequeue(IBasicProperties properties, byte[] body, ulong deliveryTag)
        {
            if (properties == null)
            {
                return null;
            }

            Interlocked.Increment(ref this.dequeuedCount);

            var item = new QueueItem<TData>(
                properties.MessageId,
                this.Serializer.Deserialize<TData>(body),
                this,
                Foundation.Extensions.FromEpoch(properties.Timestamp.UnixTime))
            {
                CorrelationId = properties.CorrelationId,
                TraceId = Encoding.UTF8.GetString((byte[])properties.Headers["TraceId"]),
                SpanId = Encoding.UTF8.GetString((byte[])properties.Headers["SpanId"])
            };
            item.Properties.Add("DeliveryTag", deliveryTag);

            using (this.Logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = item.Data.As<IHaveCorrelationId>()?.CorrelationId,
            }))
            {
                this.Logger.LogJournal(LogKeys.Queueing, $"queue item dequeued: {typeof(TData).PrettyName()} (id={item.Id}, queue={this.Options.QueueName})", LogPropertyKeys.TrackDequeue);
                this.Logger.LogTrace(LogKeys.Queueing, item.Id, typeof(TData).PrettyName(), LogTraceNames.Queue, DateTime.UtcNow - item.EnqueuedDate);
                this.LastDequeuedDate = DateTime.UtcNow;

                return item;
            }
        }
    }
}

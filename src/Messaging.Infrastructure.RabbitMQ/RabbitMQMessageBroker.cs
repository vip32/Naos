namespace Naos.Messaging.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;
    using Polly;

    public class RabbitMQMessageBroker : IMessageBroker, IDisposable
    {
        private readonly RabbitMQMessageBrokerOptions options;
        private readonly ILogger<RabbitMQMessageBroker> logger;
        private readonly ISerializer serializer;
        private IModel channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQMessageBroker"/> class.
        ///
        /// General dotnet rabbitmw docs: https://www.rabbitmq.com/dotnet-api-guide.html
        /// <para>
        ///
        /// Direct exchange behaving as fanout (pub/sub): https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html
        /// Multiple bindings:
        /// - single exchange (naos_messaging)
        /// - multiple bindings with different binding keys, per msg name (exchange fan out)
        /// - service bound queues (descriptor) with single subscriber (no round robing)
        ///
        ///                                       .-----------.         .------------.
        ///                              .------->| Queue 1   |-------->| Consumer 1 |
        ///                  bindkey=msg/name     |           |         |            |
        ///                            /  .------>|           |         |            | single consumer
        ///             .-----------. /  /        "-----------"         "------------"
        /// .---.       | Exchange  |/  /           name=svc descriptor
        /// |msg|---->  |           |--"
        /// "---"       |           |\
        ///  routkey=   "-----------" \           .-----------.         .------------.
        ///   msg name      bindkey=msg\name      | Queue 2   |-------->| Consumer 2 |
        ///                             "-------->|           |         |            |
        ///                                       |           |         |            |--.
        ///                                       "-----------"         "------------"  |
        ///                                        name=svc descriptor     | Consumer 3 | multiple consumers
        ///                                                                |            | =round-robin
        ///                                                                "------------"
        /// </para>
        /// </summary>
        /// <param name="options"></param>
        public RabbitMQMessageBroker(RabbitMQMessageBrokerOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));
            EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));
            EnsureArg.IsNotNullOrEmpty(options.ExchangeName, nameof(options.ExchangeName));
            EnsureArg.IsNotNullOrEmpty(options.QueueName, nameof(options.QueueName));

            this.options = options;
            this.logger = options.CreateLogger<RabbitMQMessageBroker>();
            this.serializer = this.options.Serializer ?? DefaultSerializer.Create;

            this.channel = this.CreateChannel(options.QueueName);
        }

        public RabbitMQMessageBroker(Builder<RabbitMQMessageBrokerOptionsBuilder, RabbitMQMessageBrokerOptions> optionsBuilder)
            : this(optionsBuilder(new RabbitMQMessageBrokerOptionsBuilder()).Build())
        {
            // TODO: maybe use this client/provider https://github.com/EasyNetQ/EasyNetQ  http://easynetq.com/
        }

        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            var routingKey = this.GetRoutingKey(messageName);

            if (!this.options.Subscriptions.Exists<TMessage>())
            {
                this.logger.LogJournal(LogKeys.AppMessaging, $"message subscribe: {messageName} (service={{Service}}, filterScope={{FilterScope}}, handler={{MessageHandlerType}})", LogPropertyKeys.TrackSubscribeMessage, args: new[] { this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name });

                if (!this.options.Provider.IsConnected)
                {
                    this.options.Provider.TryConnect();
                }

                this.logger.LogDebug($"{{LogKey:l}} bind rabbitmq queue (queue={this.options.QueueName}, routingKey={routingKey})", LogKeys.AppMessaging);
                try
                {
                    using (var channel = this.options.Provider.CreateModel())
                    {
                        channel.QueueBind(
                            exchange: this.options.ExchangeName,
                            queue: this.options.QueueName,
                            routingKey: routingKey);
                    }

                    this.options.Subscriptions.Add<TMessage, THandler>();
                    this.StartBasicConsume(this.options.QueueName);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{{LogKey:l}} subscribe failed (queue={this.options.QueueName}, routingKey={routingKey}) {ex.Message}", LogKeys.AppMessaging);
                }
            }

            return this;
        }

        public void Publish(Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if (message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = IdGenerator.Instance.Next;
            }

            var messageName = message.GetType().PrettyName();
            var routingKey = this.GetRoutingKey(messageName);

            using (this.logger.BeginScope(new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = message.CorrelationId
            }))
            using (var scope = this.options.Tracer?.BuildSpan(messageName, LogKeys.AppMessaging, SpanKind.Producer).Activate(this.logger))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = IdGenerator.Instance.Next;
                    this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogKeys.AppMessaging);
                }

                if (message.Origin.IsNullOrEmpty())
                {
                    message.Origin = this.options.MessageScope;
                    this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogKeys.AppMessaging);
                }

                if (!this.options.Provider.IsConnected)
                {
                    this.options.Provider.TryConnect();
                }

                var policy = Policy.Handle<BrokerUnreachableException>()
                    .Or<SocketException>()
                    .WaitAndRetry(this.options.Retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        this.logger.LogWarning(ex, "{LogKey:l} could not publish message: {MessageId} after {Timeout}s ({ExceptionMessage})", LogKeys.AppMessaging, message.Id, $"{time.TotalSeconds:n1}", ex.Message);
                    });

                using (var channel = this.options.Provider.CreateModel())
                {
                    var rabbitMQMessage = this.serializer.SerializeToBytes(message);

                    this.logger.LogJournal(LogKeys.AppMessaging, $"message publish: {messageName} (id={{MessageId}}, origin={{MessageOrigin}}, size={rabbitMQMessage.Length.Bytes():#.##})", LogPropertyKeys.TrackPublishMessage, args: new[] { message.Id, message.Origin });
                    this.logger.LogTrace(LogKeys.AppMessaging, message.Id, messageName, LogTraceNames.Message);

                    channel.ExchangeDeclare(exchange: this.options.ExchangeName, type: "direct");
                    policy.Execute(() =>
                    {
                        var properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 2; // persistent
                        properties.Persistent = true;
                        properties.AppId = message.Origin;
                        properties.Type = messageName;
                        properties.MessageId = message.Id;
                        properties.CorrelationId = message.CorrelationId;
                        if (this.options.Expiration.HasValue && this.options.Expiration.Value.Milliseconds > 0)
                        {
                            properties.Expiration = this.options.Expiration.Value.Milliseconds.ToString(); // https://www.rabbitmq.com/ttl.html
                        }

                        if (scope?.Span != null)
                        {
                            // propagate the span infos
                            properties.Headers ??= new Dictionary<string, object>();
                            properties.Headers.Add("TraceId", scope.Span.TraceId);
                            properties.Headers.Add("SpanId", scope.Span.SpanId);
                        }

                        channel.BasicPublish(
                            exchange: this.options.ExchangeName,
                            routingKey: routingKey,
                            mandatory: true,
                            basicProperties: properties,
                            body: rabbitMQMessage);
                    });
                }

                // TODO: async publish!
                if (this.options.Mediator != null)
                {
                    /*await */
                    this.options.Mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/
                }
            }
        }

        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            var routingKey = this.GetRoutingKey(messageName);

            this.logger.LogInformation("{LogKey:l} (name={MessageName}, orgin={MessageOrigin}, filterScope={FilterScope}, handler={MessageHandlerType})", LogKeys.AppMessaging, messageName, this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name);

            if (!this.options.Provider.IsConnected)
            {
                this.options.Provider.TryConnect();
            }

            using (var channel = this.options.Provider.CreateModel())
            {
                this.logger.LogInformation($"{{LogKey:l}} unbind rabbitmq queue (queue={this.options.QueueName}, routingKey={routingKey})", LogKeys.AppMessaging);

                channel.QueueUnbind(
                    exchange: this.options.ExchangeName,
                    queue: this.options.QueueName,
                    routingKey: routingKey);
            }

            this.options.Subscriptions.Remove<TMessage, THandler>();
        }

        public void Dispose()
        {
            this.channel?.Dispose();
            this.options?.Subscriptions?.Clear();
        }

        private string GetRoutingKey(string messageName)
        {
            var ruleName = messageName;

            if (!this.options.FilterScope.IsNullOrEmpty())
            {
                ruleName += $"-{this.options.FilterScope}";
            }

            return ruleName; //.Replace("<", "_").Replace(">", "_");
        }

        private IModel CreateChannel(string queueName)
        {
            if (!this.options.Provider.IsConnected)
            {
                this.options.Provider.TryConnect();
            }

            this.logger.LogInformation($"{{LogKey:l}} declare rabbitmq consumer channel (exchange={this.options.ExchangeName}, queue={queueName})", LogKeys.AppMessaging);

            try
            {
                var channel = this.options.Provider.CreateModel();
                channel.ExchangeDeclare(exchange: this.options.ExchangeName, type: "direct"); /*durable: true*/
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                channel.CallbackException += (sender, ea) =>
                {
                    this.logger.LogWarning($"{{LogKey:l}} recreate rabbitmq consumer channel (queue={queueName})", LogKeys.AppMessaging);

                    this.channel.Dispose();
                    this.channel = this.CreateChannel(queueName);
                    this.StartBasicConsume(queueName);
                };

                return channel;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{{LogKey:l}} rabbitmq channel cannot be created (exchange={this.options.ExchangeName}, queue={queueName})", LogKeys.AppMessaging);
                return null;
            }
        }

        private void StartBasicConsume(string queueName)
        {
            this.logger.LogInformation($"{{LogKey:l}} start rabbitmq consume (exchange={this.options.ExchangeName}, queue={queueName})", LogKeys.AppMessaging);

            if (this.channel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(this.channel);
                consumer.Received += this.Message_Received;

                this.channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                this.logger.LogError($"{{LogKey:l}} start rabbitmq consume cannot operate on empty channel (exchange={this.options.ExchangeName}, queue={queueName})", LogKeys.AppMessaging);
            }
        }

        private async Task Message_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var messageName = eventArgs.RoutingKey;
            //var rabbitMQMessage = Encoding.UTF8.GetString(eventArgs.Body);

            try
            {
                if (await this.ProcessMessage(messageName, eventArgs).AnyContext())
                {
                    // Even on exception message is taken off the queue
                    // in a REAL WORLD service this should be handled with a Dead Letter Exchange (DLX)
                    this.channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    // TODO: dead letter in case of exception https://www.rabbitmq.com/dlx.html
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{{LogKey:l}} error processing rabbitmq message (name={messageName}, id={eventArgs.BasicProperties.MessageId})", LogKeys.AppMessaging);
            }
        }

        private async Task<bool> ProcessMessage(string messageName, BasicDeliverEventArgs eventArgs)
        {
            this.logger.LogDebug($"{{LogKey:l}} processing rabbitmq message (name={messageName}, id={eventArgs.BasicProperties.MessageId})", LogKeys.AppMessaging);
            var processed = false;

            if (this.options.Subscriptions.Exists(messageName))
            {
                foreach (var subscription in this.options.Subscriptions.GetAll(messageName))
                {
                    var messageType = this.options.Subscriptions.GetByName(messageName);
                    if (messageType == null)
                    {
                        continue;
                    }

                    // get parent span infos from message
                    ISpan parentSpan = null;
                    if (eventArgs.BasicProperties?.Headers?.ContainsKey("TraceId") == true && eventArgs.BasicProperties?.Headers?.ContainsKey("SpanId") == true)
                    {
                        // dehydrate parent span
                        parentSpan = new Span(
                            Encoding.UTF8.GetString((byte[])eventArgs.BasicProperties.Headers["TraceId"]), // headers values are in bytes, convert to string
                            Encoding.UTF8.GetString((byte[])eventArgs.BasicProperties.Headers["SpanId"]));
                    }

                    using (this.logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogPropertyKeys.CorrelationId] = eventArgs.BasicProperties.CorrelationId,
                    }))
                    using (var scope = this.options.Tracer?.BuildSpan(messageName, LogKeys.AppMessaging, SpanKind.Consumer, parentSpan).Activate(this.logger))
                    {
                        // map some message properties to the typed message
                        if (!(this.serializer.Deserialize(eventArgs.Body, messageType) is Domain.Message message))
                        {
                            return false;
                        }

                        message.Origin ??= eventArgs.BasicProperties.AppId;

                        this.logger.LogJournal(LogKeys.AppMessaging, $"message processed: {eventArgs.BasicProperties.Type} (id={{MessageId}}, service={{Service}}, origin={{MessageOrigin}}, size={eventArgs.Body.Length.Bytes():#.##})",
                            LogPropertyKeys.TrackReceiveMessage, args: new[] { message?.Id, message.Origin, message.Origin });
                        //this.logger.LogTrace(LogKeys.Messaging, message.Id, eventArgs.BasicProperties.Type, LogTraceNames.Message);

                        // construct the handler by using the DI container
                        var handler = this.options.HandlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                        var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                        var method = concreteType.GetMethod("Handle");
                        if (handler != null && method != null)
                        {
                            if (this.options.Mediator != null)
                            {
                                await this.options.Mediator.Publish(new MessageHandledDomainEvent(message, message.Origin)).AnyContext();
                            }

                            await ((Task)method.Invoke(handler, new object[] { message as object })).AnyContext();
                        }
                        else
                        {
                            this.logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                                LogKeys.AppMessaging, eventArgs.BasicProperties.Type, message.Origin, message.Id, message.Origin);
                        }
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogWarning($"{{LogKey:l}} could not process rabbitmq message, no subscription exists (name={messageName}, id={eventArgs.BasicProperties.MessageId})", LogKeys.AppMessaging);
            }

            return processed;
        }
    }
}

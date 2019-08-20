namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class ServiceBusMessageBroker : IMessageBroker
    {
        private readonly ServiceBusMessageBrokerOptions options;
        private readonly ILogger<ServiceBusMessageBroker> logger;
        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessageBroker"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public ServiceBusMessageBroker(ServiceBusMessageBrokerOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));
            EnsureArg.IsNotNull(options.Client, nameof(options.Client));
            EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));
            EnsureArg.IsNotNullOrEmpty(options.SubscriptionName, nameof(options.SubscriptionName));

            this.options = options;
            this.logger = options.CreateLogger<ServiceBusMessageBroker>();
            this.serializer = this.options.Serializer ?? DefaultSerializer.Create;

            //this.client = this.ClientFactory(options.Provider, options.Provider.ConnectionStringBuilder.EntityPath, options.SubscriptionName);
            //this.RegisterMessageHandler(this.options.Client);
        }

        public ServiceBusMessageBroker(Builder<ServiceBusMessageBrokerOptionsBuilder, ServiceBusMessageBrokerOptions> optionsBuilder)
            : this(optionsBuilder(new ServiceBusMessageBrokerOptionsBuilder()).Build())
        {
        }

        /// <summary>
        /// Subscribes for the message (TMessage) with a specific message handler (THandler).
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Domain.Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            var ruleName = this.GetRuleName(messageName);

            if(!this.options.Subscriptions.Exists<TMessage>())
            {
                this.logger.LogJournal(LogKeys.Messaging, "subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, entityPath={EntityPath})", LogPropertyKeys.TrackSubscribeMessage, args: new[] { messageName, this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name, this.options.Provider.EntityPath });

                try
                {
                    this.logger.LogInformation($"{{LogKey:l}} servicebus add subscription rule: {ruleName} (name={messageName}, type={typeof(TMessage).Name})", LogKeys.Messaging);
                    this.options.Client.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter { Label = messageName, To = this.options.FilterScope }, // filterscope ist used to lock the rule for a specific machine
                        Name = ruleName
                    }).GetAwaiter().GetResult();
                }
                catch(ServiceBusException)
                {
                    this.logger.LogDebug($"{{LogKey:l}} servicebus found subscription rule: {ruleName}", LogKeys.Messaging);
                }

                this.options.Subscriptions.Add<TMessage, THandler>();
            }

            return this;
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Publish(Domain.Message message) // TODO: provide (parent) SPAN here? as ITracer is scope and this broker is singleton
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if(message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = IdGenerator.Instance.Next;
            }

            var messageName = message.GetType().PrettyName();
            using(var scope = this.options.Tracer?.BuildSpan(messageName, LogKeys.Messaging, SpanKind.Producer).Activate(this.logger))
            {
                using(this.logger.BeginScope(new Dictionary<string, object>
                {
                    [LogPropertyKeys.CorrelationId] = message.CorrelationId
                }))
                {
                    if(message.Id.IsNullOrEmpty())
                    {
                        message.Id = IdGenerator.Instance.Next;
                        this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogKeys.Messaging);
                    }

                    if(message.Origin.IsNullOrEmpty())
                    {
                        message.Origin = this.options.MessageScope;
                        this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogKeys.Messaging);
                    }

                    // TODO: async publish!
                    if(this.options.Mediator != null)
                    {
                        /*await */
                        this.options.Mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/
                    }

                    // TODO: really need non-async Result?
                    var serviceBusMessage = new Microsoft.Azure.ServiceBus.Message
                    {
                        Label = messageName,
                        MessageId = message.Id,
                        CorrelationId = message.CorrelationId.IsNullOrEmpty() ? IdGenerator.Instance.Next : message.CorrelationId,
                        //Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), // TODO: use ISerializer here, compacter messages
                        Body = this.serializer.SerializeToBytes(message),
                        To = this.options.FilterScope
                    };
                    serviceBusMessage.UserProperties.AddOrUpdate("Origin", this.options.MessageScope);
                    // propagate the span infos
                    if(scope?.Span != null)
                    {
                        serviceBusMessage.UserProperties.AddOrUpdate("TraceId", scope.Span.TraceId);
                        serviceBusMessage.UserProperties.AddOrUpdate("SpanId", scope.Span.SpanId);
                    }

                    this.logger.LogJournal(LogKeys.Messaging, $"publish (name={{MessageName}}, id={{MessageId}}, origin={{MessageOrigin}}, size={serviceBusMessage.Body.Length.Bytes().ToString("#.##")})", LogPropertyKeys.TrackPublishMessage, args: new[] { messageName, message.Id, message.Origin });
                    this.logger.LogTrace(LogKeys.Messaging, message.Id, messageName, LogTraceNames.Message);

                    this.options.Provider.TopicClientFactory().SendAsync(serviceBusMessage).GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// Unsubscribes message (TMessage) and its message handler (THandler).
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the message handler.</typeparam>
        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Domain.Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            var ruleName = this.GetRuleName(messageName);

            this.logger.LogInformation("{LogKey:l} (name={MessageName}, orgin={MessageOrigin}, filterScope={FilterScope}, handler={MessageHandlerType})", LogKeys.Messaging, messageName, this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name);

            try
            {
                this.logger.LogInformation($"{{LogKey:l}} servicebus remove subscription rule: {ruleName}", LogKeys.Messaging);
                this.options.Client
                 .RemoveRuleAsync(ruleName)
                 .GetAwaiter()
                 .GetResult();
            }
            catch(MessagingEntityNotFoundException)
            {
                this.logger.LogDebug($"{{LogKey:l}} servicebus subscription rule not found: {ruleName}", LogKeys.Messaging);
            }

            this.options.Subscriptions.Remove<TMessage, THandler>();
        }

        private string GetRuleName(string messageName)
        {
            var ruleName = messageName;

            if(!this.options.FilterScope.IsNullOrEmpty())
            {
                ruleName += $"-{this.options.FilterScope}";
            }

            return ruleName.Replace("<", "_").Replace(">", "_");
        }

#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1202 // Elements should be ordered by access
        /// <summary>
        /// Processes the message by invoking the message handler.
        /// </summary>
        public static async Task<bool> ProcessMessage(
            ILogger<ServiceBusMessageBroker> logger,
            ITracer tracer,
            ISubscriptionMap subscriptions,
            IMessageHandlerFactory handlerFactory,
            ISerializer serializer,
            string messageScope,
            IMediator mediator,
            Microsoft.Azure.ServiceBus.Message serviceBusMessage)
#pragma warning restore SA1204 // Static elements should appear before instance elements
#pragma warning restore SA1202 // Elements should be ordered by access
        {
            var processed = false;
            var messageName = serviceBusMessage.Label;

            if(subscriptions.Exists(messageName))
            {
                foreach(var subscription in subscriptions.GetAll(messageName))
                {
                    var messageType = subscriptions.GetByName(messageName);
                    if(messageType == null)
                    {
                        continue;
                    }

                    // get parent span infos from message
                    var traceId = serviceBusMessage.UserProperties.ContainsKey("TraceId") ? serviceBusMessage.UserProperties["TraceId"] as string : string.Empty;
                    var spanId = serviceBusMessage.UserProperties.ContainsKey("SpanId") ? serviceBusMessage.UserProperties["SpanId"] as string : string.Empty;
                    var parentSpan = new Span(traceId, spanId);

                    using(var scope = tracer?.BuildSpan(messageName, LogKeys.Messaging, SpanKind.Consumer, parentSpan).Activate(logger))
                    {
                        using(logger.BeginScope(new Dictionary<string, object>
                        {
                            [LogPropertyKeys.CorrelationId] = serviceBusMessage.CorrelationId,
                            //[LogPropertyKeys.TrackId] = scope.Span.SpanId
                        }))
                        {
                            // map some message properties to the typed message
                            //var jsonMessage = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(serviceBusMessage.Body), messageType); // TODO: use ISerializer here, compacter messages
                            //var message = jsonMessage as Domain.Message;
                            var message = serializer.Deserialize(serviceBusMessage.Body, messageType) as Domain.Message;
                            // TODO: message can be null, skip
                            if(message.Origin.IsNullOrEmpty())
                            {
                                //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                                message.Origin = serviceBusMessage.UserProperties.ContainsKey("Origin") ? serviceBusMessage.UserProperties["Origin"] as string : string.Empty;
                            }

                            logger.LogJournal(LogKeys.Messaging, $"process (name={{MessageName}}, id={{MessageId}}, service={{Service}}, origin={{MessageOrigin}}, size={serviceBusMessage.Body.Length.Bytes().ToString("#.##")})",
                            LogPropertyKeys.TrackReceiveMessage, args: new[] { serviceBusMessage.Label, message?.Id, messageScope, message.Origin });
                            logger.LogTrace(LogKeys.Messaging, message.Id, serviceBusMessage.Label, LogTraceNames.Message);

                            // construct the handler by using the DI container
                            var handler = handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                            var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                            var method = concreteType.GetMethod("Handle");
                            if(handler != null && method != null)
                            {
                                if(mediator != null)
                                {
                                    await mediator.Publish(new MessageHandledDomainEvent(message, messageScope)).AnyContext();
                                }

                                await ((Task)method.Invoke(handler, new object[] { message as object })).AnyContext();
                            }
                            else
                            {
                                logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                                    LogKeys.Messaging, serviceBusMessage.Label, messageScope, message.Id, message.Origin);
                            }
                        }
                    }
                }

                processed = true;
            }
            else
            {
                logger.LogDebug($"{{LogKey:l}} unprocessed: {messageName}", LogKeys.Messaging);
            }

            return processed;
        }

        //private ISubscriptionClient ClientFactory(IServiceBusProvider provider, string topicName, string subscriptionName)
        //{
        //    provider.EnsureTopicSubscription(topicName, subscriptionName);
        //    var client = new SubscriptionClient(provider.ConnectionStringBuilder, subscriptionName);

        //    this.logger.LogInformation($"{{LogKey:l}} servicebus initialize (topic={topicName}, subscription={subscriptionName})", LogKeys.Messaging);

        //    try
        //    {
        //        client
        //         .RemoveRuleAsync(RuleDescription.DefaultRuleName)
        //         .GetAwaiter()
        //         .GetResult();
        //    }
        //    catch(MessagingEntityNotFoundException)
        //    {
        //        // do nothing, default rule not found
        //    }

        //    return client;
        //}

        //private void RegisterMessageHandler(ISubscriptionClient client)
        //{
        //    client.RegisterMessageHandler(
        //        async (m, t) =>
        //        {
        //            //this.logger.LogInformation("message received (id={MessageId}, name={MessageName})", message.MessageId, message.Label);

        //            if(await ProcessMessage(this.logger, this.options.Subscriptions, this.options.HandlerFactory, this.serializer, this.options.MessageScope, this.options.Mediator, m))
        //            {
        //                // complete message so it is not received again
        //                await client.CompleteAsync(m.SystemProperties.LockToken);
        //            }
        //        },
        //        new MessageHandlerOptions(this.ExceptionReceivedHandler)
        //        {
        //            MaxConcurrentCalls = 10,
        //            AutoComplete = false,
        //            MaxAutoRenewDuration = new TimeSpan(0, 5, 0)
        //        });

        //    this.logger.LogInformation("{LogKey:l} servicebus handler registered", LogKeys.Messaging);
        //}

        //private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        //{
        //    var context = args.ExceptionReceivedContext;
        //    this.logger.LogWarning($"{{LogKey:l}} servicebus handler error: topic={context?.EntityPath}, action={context?.Action}, endpoint={context?.Endpoint}, {args.Exception?.Message}, {args.Exception?.StackTrace}", LogKeys.Messaging);
        //    return Task.CompletedTask;
        //}
    }
}

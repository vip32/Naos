namespace Naos.Messaging.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class ServiceBusMessageBroker : IMessageBroker, IDisposable
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

            if (!this.options.Subscriptions.Exists<TMessage>())
            {
                this.logger.LogJournal(LogKeys.AppMessaging, $"message subscribe: {messageName} (service={{Service}}, filterScope={{FilterScope}}, handler={{MessageHandlerType}}, entityPath={this.options.Provider.EntityPath})", LogPropertyKeys.TrackSubscribeMessage, args: new[] { this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name });

                try
                {
                    this.logger.LogDebug($"{{LogKey:l}} add servicebus subscription rule: {ruleName} (name={messageName}, type={typeof(TMessage).Name})", LogKeys.AppMessaging);
                    this.options.Client.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter { Label = messageName, To = this.options.FilterScope }, // filterscope ist used to lock the rule for a specific machine
                        Name = ruleName
                    }).GetAwaiter().GetResult();
                }
                catch (ServiceBusException)
                {
                    this.logger.LogDebug($"{{LogKey:l}} servicebus found subscription rule: {ruleName}", LogKeys.AppMessaging);
                }

                this.options.Subscriptions.Add<TMessage, THandler>();
            }

            return this;
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Publish(Domain.Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if (message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = IdGenerator.Instance.Next;
            }

            var messageName = message.GetType().PrettyName();
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

                // TODO: async publish!
                if (this.options.Mediator != null)
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
                serviceBusMessage.UserProperties.AddOrUpdate("Origin", message.Origin);
                if (scope?.Span != null)
                {
                    // propagate the span infos
                    serviceBusMessage.UserProperties.AddOrUpdate("TraceId", scope.Span.TraceId);
                    serviceBusMessage.UserProperties.AddOrUpdate("SpanId", scope.Span.SpanId);
                }

                this.logger.LogJournal(LogKeys.AppMessaging, $"message publish: {messageName} (id={{MessageId}}, origin={{MessageOrigin}}, size={serviceBusMessage.Body.Length.Bytes():#.##})", LogPropertyKeys.TrackPublishMessage, args: new[] { message.Id, message.Origin });
                this.logger.LogTrace(LogKeys.AppMessaging, message.Id, messageName, LogTraceNames.Message);

                this.options.Provider.TopicClientFactory().SendAsync(serviceBusMessage).GetAwaiter().GetResult();
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

            this.logger.LogInformation("{LogKey:l} (name={MessageName}, orgin={MessageOrigin}, filterScope={FilterScope}, handler={MessageHandlerType})", LogKeys.AppMessaging, messageName, this.options.MessageScope, this.options.FilterScope, typeof(THandler).Name);

            try
            {
                this.logger.LogInformation($"{{LogKey:l}} remove servicebus subscription rule: {ruleName}", LogKeys.AppMessaging);
                this.options.Client
                    .RemoveRuleAsync(ruleName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                this.logger.LogDebug($"{{LogKey:l}} servicebus subscription rule not found: {ruleName}", LogKeys.AppMessaging);
            }

            this.options.Subscriptions.Remove<TMessage, THandler>();
        }

        public void Dispose()
        {
            this.options?.Subscriptions?.Clear();
        }

        private string GetRuleName(string messageName)
        {
            var ruleName = messageName;

            if (!this.options.FilterScope.IsNullOrEmpty())
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

            if (subscriptions.Exists(messageName))
            {
                foreach (var subscription in subscriptions.GetAll(messageName))
                {
                    var messageType = subscriptions.GetByName(messageName);
                    if (messageType == null)
                    {
                        continue;
                    }

                    // get parent span infos from message
                    ISpan parentSpan = null;
                    if (serviceBusMessage.UserProperties.ContainsKey("TraceId") && serviceBusMessage.UserProperties.ContainsKey("SpanId"))
                    {
                        // dehydrate parent span
                        parentSpan = new Span(serviceBusMessage.UserProperties["TraceId"] as string, serviceBusMessage.UserProperties["SpanId"] as string);
                    }

                    using (logger.BeginScope(new Dictionary<string, object>
                    {
                        [LogPropertyKeys.CorrelationId] = serviceBusMessage.CorrelationId,
                        //[LogPropertyKeys.TrackId] = scope.Span.SpanId = allready done in Span ScopeManager (activate)
                    }))
                    using (var scope = tracer?.BuildSpan(messageName, LogKeys.AppMessaging, SpanKind.Consumer, parentSpan).Activate(logger))
                    {
                        // map some message properties to the typed message
                        //var jsonMessage = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(serviceBusMessage.Body), messageType); // TODO: use ISerializer here, compacter messages
                        //var message = jsonMessage as Domain.Message;
                        if (!(serializer.Deserialize(serviceBusMessage.Body, messageType) is Domain.Message message))
                        {
                            return false;
                        }

                        // TODO: message can be null, skip
                        if (message.Origin.IsNullOrEmpty())
                        {
                            //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                            message.Origin = serviceBusMessage.UserProperties.ContainsKey("Origin") ? serviceBusMessage.UserProperties["Origin"] as string : string.Empty;
                        }

                        logger.LogJournal(LogKeys.AppMessaging, $"message processed: {serviceBusMessage.Label} (id={{MessageId}}, service={{Service}}, origin={{MessageOrigin}}, size={serviceBusMessage.Body.Length.Bytes():#.##})",
                            LogPropertyKeys.TrackReceiveMessage, args: new[] { message?.Id, messageScope, message.Origin });
                        logger.LogTrace(LogKeys.AppMessaging, message.Id, serviceBusMessage.Label, LogTraceNames.Message);

                        // construct the handler by using the DI container
                        var handler = handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                        var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                        var method = concreteType.GetMethod("Handle");
                        if (handler != null && method != null)
                        {
                            if (mediator != null)
                            {
                                await mediator.Publish(new MessageHandledDomainEvent(message, messageScope)).AnyContext();
                            }

                            await ((Task)method.Invoke(handler, new object[] { message as object })).AnyContext();
                        }
                        else
                        {
                            logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                                LogKeys.AppMessaging, serviceBusMessage.Label, messageScope, message.Id, message.Origin);
                        }
                    }
                }

                processed = true;
            }
            else
            {
                logger.LogDebug($"{{LogKey:l}} unprocessed: {messageName}", LogKeys.AppMessaging);
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

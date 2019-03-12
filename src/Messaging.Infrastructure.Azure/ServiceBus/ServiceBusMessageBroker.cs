namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using EnsureThat;
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Infrastructure.Azure.ServiceBus;
    using Naos.Core.Messaging.Domain;
    using Newtonsoft.Json;

    public class ServiceBusMessageBroker : IMessageBroker
    {
        private readonly ILogger<ServiceBusMessageBroker> logger;
        private readonly IMediator mediator;
        private readonly IServiceBusProvider provider;
        private readonly ISubscriptionMap map;
        private readonly IMessageHandlerFactory handlerFactory;
        private readonly string messageScope;
        private readonly string filterScope;
        private SubscriptionClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessageBroker"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public ServiceBusMessageBroker(ServiceBusMessageBrokerOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.LoggerFactory, nameof(options.LoggerFactory));
            EnsureArg.IsNotNull(options.Mediator, nameof(options.Mediator));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));
            EnsureArg.IsNotNull(options.HandlerFactory, nameof(options.HandlerFactory));
            EnsureArg.IsNotNullOrEmpty(options.SubscriptionName, nameof(options.SubscriptionName));

            this.logger = options.LoggerFactory.CreateLogger<ServiceBusMessageBroker>();
            this.mediator = options.Mediator;
            this.provider = options.Provider;
            this.map = options.Map ?? new SubscriptionMap();
            this.handlerFactory = options.HandlerFactory;
            this.filterScope = options.FilterScope; // for machine scope
            this.messageScope = options.MessageScope ?? options.SubscriptionName; // message origin service name

            this.InitializeClient(this.provider, this.provider.ConnectionStringBuilder.EntityPath, options.SubscriptionName);
            this.RegisterMessageHandler();
        }

        public ServiceBusMessageBroker(Builder<ServiceBusMessageBrokerOptionsBuilder, ServiceBusMessageBrokerOptions> config)
            : this(config(new ServiceBusMessageBrokerOptionsBuilder()).Build())
        {
        }

        /// <summary>
        /// Subscribes for the message (TMessage) with a specific message handler (THandler)
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <returns></returns>
        public IMessageBroker Subscribe<TMessage, THandler>()
            where TMessage : Domain.Model.Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            string ruleName = this.GetRuleName(messageName);

            if (!this.map.Exists<TMessage>())
            {
                this.logger.LogInformation("{LogKey:l} subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType}, entityPath={EntityPath})", LogEventKeys.Messaging, messageName, this.messageScope, this.filterScope, typeof(THandler).Name, this.provider.EntityPath);

                try
                {
                    this.logger.LogInformation($"{{LogKey:l}} servicebus add subscription rule: {ruleName} (name={messageName}, type={typeof(TMessage).Name})", LogEventKeys.Messaging);
                    this.client.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter { Label = messageName, To = this.filterScope }, // filterscope ist used to lock the rule for a specific machine
                        Name = ruleName
                    }).GetAwaiter().GetResult();
                }
                catch (ServiceBusException)
                {
                    this.logger.LogDebug($"{{LogKey:l}} servicebus found subscription rule: {ruleName}", LogEventKeys.Messaging);
                }

                this.map.Add<TMessage, THandler>();
            }

            return this;
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Publish(Domain.Model.Message message)
        {
            EnsureArg.IsNotNull(message, nameof(message));
            if (message.CorrelationId.IsNullOrEmpty())
            {
                message.CorrelationId = RandomGenerator.GenerateString(13, true); //Guid.NewGuid().ToString().Replace("-", string.Empty);
            }

            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.logger.BeginScope(loggerState))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"{{LogKey:l}} set message (id={message.Id})", LogEventKeys.Messaging);
                }

                if (message.Origin.IsNullOrEmpty())
                {
                    message.Origin = this.messageScope;
                    this.logger.LogDebug($"{{LogKey:l}} set message (origin={message.Origin})", LogEventKeys.Messaging);
                }

                // TODO: async publish!
                /*await */ this.mediator.Publish(new MessagePublishedDomainEvent(message)).GetAwaiter().GetResult(); /*.AnyContext();*/

                var messageName = /*message.Name*/ message.GetType().PrettyName();

                this.logger.LogInformation("{LogKey:l} publish (name={MessageName}, id={MessageId}, origin={MessageOrigin})", LogEventKeys.Messaging, messageName, message.Id, message.Origin);

                // TODO: really need non-async Result?
                var serviceBusMessage = new Message
                {
                    Label = messageName,
                    MessageId = message.Id,
                    CorrelationId = message.CorrelationId.IsNullOrEmpty() ? Guid.NewGuid().ToString() : message.CorrelationId,
                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)),
                    To = this.filterScope
                };
                serviceBusMessage.UserProperties.AddOrUpdate("Origin", this.messageScope);

                this.provider.CreateModel().SendAsync(serviceBusMessage).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Unsubscribes message (TMessage) and its message handler (THandler)
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the message handler.</typeparam>
        public void Unsubscribe<TMessage, THandler>()
            where TMessage : Domain.Model.Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            string ruleName = this.GetRuleName(messageName);

            this.logger.LogInformation("{LogKey:l} (name={MessageName}, orgin={MessageOrigin}, filterScope={FilterScope}, handler={MessageHandlerType})", LogEventKeys.Messaging, messageName, this.messageScope, this.filterScope, typeof(THandler).Name);

            try
            {
                this.logger.LogInformation($"{{LogKey:l}} servicebus remove subscription rule: {ruleName}", LogEventKeys.Messaging);
                this.client
                 .RemoveRuleAsync(ruleName)
                 .GetAwaiter()
                 .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                this.logger.LogDebug($"{{LogKey:l}} servicebus subscription rule not found: {ruleName}", LogEventKeys.Messaging);
            }

            this.map.Remove<TMessage, THandler>();
        }

        private string GetRuleName(string messageName)
        {
            var ruleName = messageName;

            if (!this.filterScope.IsNullOrEmpty())
            {
                ruleName += $"-{this.filterScope}";
            }

            return ruleName.Replace("<", "_").Replace(">", "_");
        }

        private void RegisterMessageHandler()
        {
            this.client.RegisterMessageHandler(
                async (m, t) =>
                {
                    //this.logger.LogInformation("message received (id={MessageId}, name={MessageName})", message.MessageId, message.Label);

                    if (await this.ProcessMessage(m))
                    {
                        // complete message so it is not received again
                        await this.client.CompleteAsync(m.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(this.ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 10,
                    AutoComplete = false,
                    MaxAutoRenewDuration = new TimeSpan(0, 5, 0)
                });

            this.logger.LogInformation("{LogKey:l} servicebus handler registered", LogEventKeys.Messaging);
        }

        /// <summary>
        /// Processes the message by invoking the message handler.
        /// </summary>
        /// <param name="serviceBusMessage">The servicebus message.</param>
        /// <returns></returns>
        private async Task<bool> ProcessMessage(Message serviceBusMessage)
        {
            var processed = false;
            var messageName = serviceBusMessage.Label;
            var messageBody = Encoding.UTF8.GetString(serviceBusMessage.Body);

            if (this.map.Exists(messageName))
            {
                foreach (var subscription in this.map.GetAll(messageName))
                {
                    var messageType = this.map.GetByName(messageName);
                    if (messageType == null)
                    {
                        continue;
                    }

                    var jsonMessage = JsonConvert.DeserializeObject(messageBody, messageType);
                    var loggerState = new Dictionary<string, object>
                    {
                        [LogEventPropertyKeys.CorrelationId] = serviceBusMessage.CorrelationId,
                    };

                    using (this.logger.BeginScope(loggerState))
                    {
                        // map some message properties to the typed message
                        var message = jsonMessage as Domain.Model.Message;
                        if (message?.Origin.IsNullOrEmpty() == true)
                        {
                            //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                            message.Origin = serviceBusMessage.UserProperties.ContainsKey("Origin") ? serviceBusMessage.UserProperties["Origin"] as string : string.Empty;
                        }

                        this.logger.LogInformation("{LogKey:l} process (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                            LogEventKeys.Messaging, serviceBusMessage.Label, message?.Id, this.messageScope, message.Origin);

                        // construct the handler by using the DI container
                        var handler = this.handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                        var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                        var method = concreteType.GetMethod("Handle");
                        if (handler != null && method != null)
                        {
                            await this.mediator.Publish(new MessageHandledDomainEvent(message, this.messageScope)).AnyContext();
                            await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                        }
                        else
                        {
                            this.logger.LogWarning("{LogKey:l} process failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                                LogEventKeys.Messaging, serviceBusMessage.Label, this.messageScope, jsonMessage.AsJToken().GetStringPropertyByPath("id"), message.Origin);
                        }
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"{{LogKey:l}} unprocessed: {messageName}", LogEventKeys.Messaging);
            }

            return processed;
        }

        private void InitializeClient(IServiceBusProvider provider, string topicName, string subscriptionName)
        {
            this.provider.EnsureSubscription(topicName, subscriptionName);
            this.client = new SubscriptionClient(provider.ConnectionStringBuilder, subscriptionName);

            this.logger.LogInformation($"{{LogKey:l}} servicebus initialize (topic={topicName}, subscription={subscriptionName})", LogEventKeys.Messaging);

            try
            {
                this.client
                 .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                 .GetAwaiter()
                 .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                // do nothing, default rule not found
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            var context = args.ExceptionReceivedContext;
            this.logger.LogWarning($"{{LogKey:l}} servicebus handler error: topic={context?.EntityPath}, action={context?.Action}, endpoint={context?.Endpoint}, {args.Exception?.Message}", LogEventKeys.Messaging);
            return Task.CompletedTask;
        }
    }
}

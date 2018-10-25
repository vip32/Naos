namespace Naos.Core.Messaging.Infrastructure.Azure.ServiceBus
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using EnsureThat;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class ServiceBusMessageBus : IMessageBus
    {
        private readonly ILogger<ServiceBusMessageBus> logger;
        private readonly IServiceBusProvider provider;
        private readonly ISubscriptionMap map;
        private readonly IMessageHandlerFactory handlerFactory;
        private readonly string messageScope;
        private readonly string filterScope;
        private SubscriptionClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessageBus" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="provider">The connection.</param>
        /// <param name="map">The map.</param>
        /// <param name="handlerFactory">The service provider.</param>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <param name="messageScope">The message scope.</param>
        /// <param name="filterScope">Name of the scope.</param>
        public ServiceBusMessageBus(
            ILogger<ServiceBusMessageBus> logger,
            IServiceBusProvider provider,
            IMessageHandlerFactory handlerFactory,
            string subscriptionName,
            ISubscriptionMap map = null,
            string filterScope = null,
            string messageScope = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(provider, nameof(provider));
            EnsureArg.IsNotNull(handlerFactory, nameof(handlerFactory));
            EnsureArg.IsNotNullOrEmpty(subscriptionName, nameof(subscriptionName));

            this.logger = logger;
            this.provider = provider;
            this.map = map ?? new SubscriptionMap();
            this.handlerFactory = handlerFactory;
            this.filterScope = filterScope; // for machine scope
            this.messageScope = messageScope ?? subscriptionName;

            this.InitializeClient(provider, provider.ConnectionStringBuilder.EntityPath, subscriptionName);
            this.RegisterMessageHandler();
        }

        /// <summary>
        /// Subscribes for the message (TMessage) with a specific message handler (THandler)
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        public void Subscribe<TMessage, THandler>()
            where TMessage : Domain.Model.Message
            where THandler : IMessageHandler<TMessage>
        {
            var messageName = typeof(TMessage).PrettyName();
            string ruleName = this.GetRuleName(messageName);

            if (!this.map.Exists<TMessage>())
            {
                this.logger.LogInformation("subscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType})", messageName, this.messageScope, this.filterScope, typeof(THandler).Name);

                try
                {
                    this.logger.LogInformation($"servicebus add subscription rule: {ruleName} (name={messageName}, type={typeof(TMessage).Name})");
                    this.client.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter { Label = messageName, To = this.filterScope }, // filterscope ist used to lock the rule for a specific machine
                        Name = ruleName
                    }).GetAwaiter().GetResult();
                }
                catch (ServiceBusException)
                {
                    this.logger.LogDebug($"servicebus found subscription rule: {ruleName}");
                }

                this.map.Add<TMessage, THandler>();
            }
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
                message.CorrelationId = Guid.NewGuid().ToString();
            }

            using (this.logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                if (message.Id.IsNullOrEmpty())
                {
                    message.Id = Guid.NewGuid().ToString();
                    this.logger.LogDebug($"set messageId (id={message.Id})");
                }

                var messageName = /*message.Name*/ message.GetType().PrettyName();

                this.logger.LogInformation("publish message (name={MessageName}, id={MessageId}, service={Service})", messageName, message.Id, this.messageScope);

                // TODO: really need non-async Result?
                var serviceBusMessage = new Microsoft.Azure.ServiceBus.Message
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

            this.logger.LogInformation("unsubscribe (name={MessageName}, service={Service}, filterScope={FilterScope}, handler={MessageHandlerType})", messageName, this.messageScope, this.filterScope, typeof(THandler).Name);

            try
            {
                this.logger.LogInformation($"servicebus remove subscription rule: {ruleName}");
                this.client
                 .RemoveRuleAsync(ruleName)
                 .GetAwaiter()
                 .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                this.logger.LogDebug($"servicebus subscription rule not found: {ruleName}");
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
                async (message, token) =>
                {
                    //this.logger.LogInformation("message received (id={MessageId}, name={MessageName})", message.MessageId, message.Label);

                    if (await this.ProcessMessage(message))
                    {
                        // complete message so it is not received again
                        await this.client.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(this.ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 10,
                    AutoComplete = false,
                    MaxAutoRenewDuration = new TimeSpan(0, 5, 0)
                });

            this.logger.LogInformation("servicebus message handler registered");
        }

        /// <summary>
        /// Processes the message by invoking the message handler.
        /// </summary>
        /// <param name="serviceBusMessage">The servicebus message.</param>
        /// <returns></returns>
        private async Task<bool> ProcessMessage(Microsoft.Azure.ServiceBus.Message serviceBusMessage)
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
                    using (this.logger.BeginScope("{CorrelationId}", serviceBusMessage.CorrelationId))
                    {
                        var messageOrigin = serviceBusMessage.UserProperties.ContainsKey("Origin") ? serviceBusMessage.UserProperties["Origin"] as string : string.Empty;

                        // map some message properties to the typed message
                        var message = jsonMessage as Domain.Model.Message;
                        if (message != null)
                        {
                            //message.CorrelationId = jsonMessage.AsJToken().GetStringPropertyByToken("CorrelationId");
                            message.Origin = messageOrigin;
                        }

                        this.logger.LogInformation("process message (name={MessageName}, id={MessageId}, service={Service}, origin={MessageOrigin})",
                            serviceBusMessage.Label, message?.Id, this.messageScope, messageOrigin);

                        // construct the handler by using the DI container
                        var handler = this.handlerFactory.Create(subscription.HandlerType); // should not be null, did you forget to register your generic handler (EntityMessageHandler<T>)
                        var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType);

                        var method = concreteType.GetMethod("Handle");
                        if (handler != null && method != null)
                        {
                            await (Task)method.Invoke(handler, new object[] { jsonMessage as object });
                        }
                        else
                        {
                            this.logger.LogWarning("process message failed, message handler could not be created. is the handler registered in the service provider? (name={MessageName}, service={Service}, id={MessageId}, origin={MessageOrigin})",
                                serviceBusMessage.Label, this.messageScope, jsonMessage.AsJToken().GetStringPropertyByToken("id"), messageOrigin);
                        }
                    }
                }

                processed = true;
            }
            else
            {
                this.logger.LogDebug($"unprocessed: {messageName}");
            }

            return processed;
        }

        private void InitializeClient(IServiceBusProvider provider, string topicName, string subscriptionName)
        {
            this.provider.EnsureSubscription(topicName, subscriptionName);
            this.client = new SubscriptionClient(provider.ConnectionStringBuilder, subscriptionName);

            this.logger.LogInformation($"servicebus initialize (topic={topicName}, subscription={subscriptionName}");

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
            this.logger.LogWarning($"servicebus message handler error: topic={context?.EntityPath}, action={context?.Action}, endpoint={context?.Endpoint}, {args.Exception?.Message}");
            return Task.CompletedTask;
        }
    }
}

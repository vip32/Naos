namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using MediatR;
    using Naos.Core.Messaging.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;

    public class ServiceBusMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<ServiceBusMessageBrokerOptions, ServiceBusMessageBrokerOptionsBuilder>
    {
        public ServiceBusMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder Provider(IServiceBusProvider provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder HandlerFactory(IMessageHandlerFactory handlerFactory)
        {
            this.Target.HandlerFactory = handlerFactory;
            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder SubscriptionName(string subscriptionName)
        {
            this.Target.SubscriptionName = subscriptionName;
            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder Map(ISubscriptionMap map)
        {
            this.Target.Subscriptions = map;
            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder FilterScope(string filterScope)
        {
            this.Target.FilterScope = filterScope;
            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder MessageScope(string messageScope)
        {
            this.Target.MessageScope = messageScope;
            return this;
        }
    }
}
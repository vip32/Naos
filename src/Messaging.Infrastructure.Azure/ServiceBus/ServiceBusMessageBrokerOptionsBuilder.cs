namespace Naos.Messaging.Infrastructure.Azure
{
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class ServiceBusMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<ServiceBusMessageBrokerOptions, ServiceBusMessageBrokerOptionsBuilder>
    {
        public ServiceBusMessageBrokerOptionsBuilder Tracer(ITracer tracer)
        {
            this.Target.Tracer = tracer;
            return this;
        }

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

        public ServiceBusMessageBrokerOptionsBuilder Client(ISubscriptionClient client)
        {
            this.Target.Client = client;
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
            if (this.Target.MessageScope.IsNullOrEmpty())
            {
                this.Target.MessageScope = subscriptionName;
            }

            return this;
        }

        public ServiceBusMessageBrokerOptionsBuilder Subscriptions(ISubscriptionMap subscriptions)
        {
            this.Target.Subscriptions = subscriptions;
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
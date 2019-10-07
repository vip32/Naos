namespace Naos.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class RabbitMQMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<RabbitMQMessageBrokerOptions, RabbitMQMessageBrokerOptionsBuilder>
    {
        public RabbitMQMessageBrokerOptionsBuilder Tracer(ITracer tracer)
        {
            this.Target.Tracer = tracer;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder HandlerFactory(IMessageHandlerFactory handlerFactory)
        {
            this.Target.HandlerFactory = handlerFactory;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder SubscriptionName(string subscriptionName)
        {
            this.Target.SubscriptionName = subscriptionName;
            if (this.Target.MessageScope.IsNullOrEmpty())
            {
                this.Target.MessageScope = subscriptionName;
            }

            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Subscriptions(ISubscriptionMap map)
        {
            this.Target.Subscriptions = map;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder FilterScope(string filterScope)
        {
            this.Target.FilterScope = filterScope;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder MessageScope(string messageScope)
        {
            this.Target.MessageScope = messageScope;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Provider(IRabbitMQProvider provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder ExchangeName(string value)
        {
            this.Target.ExchangeName = value;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Host(string host)
        {
            this.Target.Host = host;
            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder RetryCount(int value)
        {
            this.Target.RetryCount = value;
            return this;
        }
    }
}
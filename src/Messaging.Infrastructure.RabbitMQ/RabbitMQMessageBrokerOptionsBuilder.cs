namespace Naos.Messaging.Infrastructure
{
    using System;
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

        public RabbitMQMessageBrokerOptionsBuilder QueueName(string name)
        {
            if (!name.IsNullOrEmpty())
            {
                this.Target.QueueName = name;
                if (this.Target.MessageScope.IsNullOrEmpty())
                {
                    this.Target.MessageScope = name;
                }
            }

            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Subscriptions(ISubscriptionMap subscriptions)
        {
            this.Target.Subscriptions = subscriptions;
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
            if (!value.IsNullOrEmpty())
            {
                this.Target.ExchangeName = value;
            }

            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Retries(int? value)
        {
            if (value.HasValue)
            {
                this.Target.Retries = value.Value;
            }

            return this;
        }

        public RabbitMQMessageBrokerOptionsBuilder Expiration(TimeSpan? expiration)
        {
            if (expiration.HasValue)
            {
                this.Target.Expiration = expiration;
            }

            return this;
        }
    }
}
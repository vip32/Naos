namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Messaging.Domain;

    public class RabbitMQMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<RabbitMQMessageBrokerOptions, RabbitMQMessageBrokerOptionsBuilder>
    {
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

        public RabbitMQMessageBrokerOptionsBuilder Host(string host)
        {
            this.Target.Host = host;
            return this;
        }
    }
}
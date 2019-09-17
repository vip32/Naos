namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

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

        public RabbitMQMessageBrokerOptionsBuilder Host(string host)
        {
            this.Target.Host = host;
            return this;
        }
    }
}
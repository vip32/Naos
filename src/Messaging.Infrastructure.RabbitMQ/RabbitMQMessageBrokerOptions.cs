namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class RabbitMQMessageBrokerOptions : BaseOptions
    {
        public ITracer Tracer { get; set; }

        public IMediator Mediator { get; set; }

        public ISerializer Serializer { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public ISubscriptionMap Map { get; set; }

        public string Host { get; set; }
    }
}

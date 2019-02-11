namespace Naos.Core.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Core.Common;

    public class RabbitMQMessageBrokerOptions : BaseOptions
    {
        public IMediator Mediator { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public ISubscriptionMap Map { get; set; }

        public RabbitMQConfiguration Configuration { get; set; }
    }
}

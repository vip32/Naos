namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure.ServiceBus;
    using Naos.Core.Messaging.Domain;

    public class ServiceBusMessageBrokerOptions : BaseOptions
    {
        public IMediator Mediator { get; set; }

        public ISerializer Serializer { get; set; }

        public IServiceBusProvider Provider { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public string SubscriptionName { get; set; }

        public ISubscriptionMap Map { get; set; } = new SubscriptionMap();

        public string FilterScope { get; set; } // for machine scope

        public string MessageScope { get; set; } // message origin service name
    }
}
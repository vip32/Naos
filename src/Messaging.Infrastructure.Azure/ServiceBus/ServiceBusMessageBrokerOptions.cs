namespace Naos.Messaging.Infrastructure.Azure
{
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class ServiceBusMessageBrokerOptions : BaseOptions
    {
        public ITracer Tracer { get; set; }

        public IMediator Mediator { get; set; }

        public ISerializer Serializer { get; set; }

        public IServiceBusProvider Provider { get; set; }

        public ISubscriptionClient Client { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public string SubscriptionName { get; set; }

        public ISubscriptionMap Subscriptions { get; set; } = new SubscriptionMap();

        public string FilterScope { get; set; } // for machine scope

        public string MessageScope { get; set; } // message origin service name
    }
}
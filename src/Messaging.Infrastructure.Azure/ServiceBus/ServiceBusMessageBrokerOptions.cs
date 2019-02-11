namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure.ServiceBus;

    public class ServiceBusMessageBrokerOptions : BaseOptions
    {
        public IMediator Mediator { get; set; }

        public IServiceBusProvider Provider { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public string SubscriptionName { get; set; }

        public ISubscriptionMap Map { get; set; }

        public string FilterScope { get; set; }

        public string MessageScope { get; set; }
    }
}
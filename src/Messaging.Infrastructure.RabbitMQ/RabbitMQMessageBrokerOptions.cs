namespace Naos.Messaging.Infrastructure.RabbitMQ
{
    using MediatR;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class RabbitMQMessageBrokerOptions : OptionsBase
    {
        public ITracer Tracer { get; set; }

        public IMediator Mediator { get; set; }

        public ISerializer Serializer { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public string QueueName { get; set; } //= queue (=ServiceDescriptor)

        public ISubscriptionMap Subscriptions { get; set; } = new SubscriptionMap();

        public string FilterScope { get; set; } // for machine scope

        public string MessageScope { get; set; } // message origin service name

        public IRabbitMQProvider Provider { get; set; }

        public string ExchangeName { get; set; } = "naos_messaging";

        public int RetryCount { get; set; } = 3;
    }
}

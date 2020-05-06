namespace Naos.Messaging.Infrastructure.Azure
{
    using System.Net.Http;
    using MediatR;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class SignalRServerlessMessageBrokerOptions : OptionsBase
    {
        public ITracer Tracer { get; set; }

        public IMediator Mediator { get; set; }

        public ISerializer Serializer { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public string ConnectionString { get; set; }

        public IHttpClientFactory HttpClient { get; set; }

        public ISubscriptionMap Subscriptions { get; set; }

        public string FilterScope { get; set; }

        public string MessageScope { get; set; } = "local";
    }
}

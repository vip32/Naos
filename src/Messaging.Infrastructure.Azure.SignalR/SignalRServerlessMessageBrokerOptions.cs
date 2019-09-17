namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using System.Net.Http;
    using MediatR;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class SignalRServerlessMessageBrokerOptions : BaseOptions
    {
        public ITracer Tracer { get; set; }

        public IMediator Mediator { get; set; }

        public ISerializer Serializer { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public string ConnectionString { get; set; }

        public IHttpClientFactory HttpClient { get; set; }

        public ISubscriptionMap Map { get; set; }

        public string FilterScope { get; set; }

        public string MessageScope { get; set; } = "local";
    }
}

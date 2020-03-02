namespace Naos.Messaging.Infrastructure.Azure
{
    using System.Net.Http;
    using MediatR;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class SignalRServerlessMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<SignalRServerlessMessageBrokerOptions, SignalRServerlessMessageBrokerOptionsBuilder>
    {
        public SignalRServerlessMessageBrokerOptionsBuilder Tracer(ITracer tracer)
        {
            this.Target.Tracer = tracer;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder HandlerFactory(IMessageHandlerFactory handlerFactory)
        {
            this.Target.HandlerFactory = handlerFactory;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder HttpClient(IHttpClientFactory httpClient)
        {
            this.Target.HttpClient = httpClient;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder Subscriptions(ISubscriptionMap subscriptions)
        {
            this.Target.Subscriptions = subscriptions;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder FilterScope(string filterScope)
        {
            this.Target.FilterScope = filterScope;
            return this;
        }

        public SignalRServerlessMessageBrokerOptionsBuilder MessageScope(string messageScope)
        {
            this.Target.MessageScope = messageScope;
            return this;
        }
    }
}
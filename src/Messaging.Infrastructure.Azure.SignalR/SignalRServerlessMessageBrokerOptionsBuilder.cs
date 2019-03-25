namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using System.Net.Http;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Messaging.Domain;

    public class SignalRServerlessMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<SignalRServerlessMessageBrokerOptions, SignalRServerlessMessageBrokerOptionsBuilder>
    {
        public SignalRServerlessMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
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

        public SignalRServerlessMessageBrokerOptionsBuilder Map(ISubscriptionMap map)
        {
            this.Target.Map = map;
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
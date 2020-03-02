namespace Naos.Messaging.Infrastructure
{
    using MediatR;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class FileStorageMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<FileStorageMessageBrokerOptions, FileStorageMessageBrokerOptionsBuilder>
    {
        public FileStorageMessageBrokerOptionsBuilder Tracer(ITracer tracer)
        {
            this.Target.Tracer = tracer;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder HandlerFactory(IMessageHandlerFactory handlerFactory)
        {
            this.Target.HandlerFactory = handlerFactory;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder Storage(IFileStorage fileStorage)
        {
            this.Target.Storage = fileStorage;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder Subscriptions(ISubscriptionMap subscriptions)
        {
            this.Target.Subscriptions = subscriptions;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder FilterScope(string filterScope)
        {
            this.Target.FilterScope = filterScope;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder MessageScope(string messageScope)
        {
            this.Target.MessageScope = messageScope;
            return this;
        }

        public FileStorageMessageBrokerOptionsBuilder ProcessDelay(int processDelay)
        {
            this.Target.ProcessDelay = processDelay;
            return this;
        }
    }
}
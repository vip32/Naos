namespace Naos.Core.Messaging.Infrastructure
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileStorageMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<FileStorageMessageBrokerOptions, FileStorageMessageBrokerOptionsBuilder>
    {
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

        public FileStorageMessageBrokerOptionsBuilder Map(ISubscriptionMap map)
        {
            this.Target.Map = map;
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
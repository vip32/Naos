namespace Naos.Core.Messaging.Infrastructure
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileSystemMessageBrokerOptionsBuilder :
        BaseOptionsBuilder<FileSystemMessageBrokerOptions, FileSystemMessageBrokerOptionsBuilder>
    {
        public FileSystemMessageBrokerOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public FileSystemMessageBrokerOptionsBuilder HandlerFactory(IMessageHandlerFactory handlerFactory)
        {
            this.Target.HandlerFactory = handlerFactory;
            return this;
        }

        public FileSystemMessageBrokerOptionsBuilder Storage(IFileStorage fileStorage)
        {
            this.Target.Storage = fileStorage;
            return this;
        }

        public FileSystemMessageBrokerOptionsBuilder Configuration(FileSystemConfiguration configuration)
        {
            this.Target.Configuration = configuration;
            return this;
        }

        public FileSystemMessageBrokerOptionsBuilder Map(ISubscriptionMap map)
        {
            this.Target.Map = map;
            return this;
        }

        public FileSystemMessageBrokerOptionsBuilder FilterScope(string filterScope)
        {
            this.Target.FilterScope = filterScope;
            return this;
        }

        public FileSystemMessageBrokerOptionsBuilder MessageScope(string messageScope)
        {
            this.Target.MessageScope = messageScope;
            return this;
        }
    }
}
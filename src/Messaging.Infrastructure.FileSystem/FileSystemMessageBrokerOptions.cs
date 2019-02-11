namespace Naos.Core.Messaging.Infrastructure
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileSystemMessageBrokerOptions : BaseOptions
    {
        public IMediator Mediator { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public IFileStorage Storage { get; set; }

        public FileSystemConfiguration Configuration { get; set; }

        public ISubscriptionMap Map { get; set; }

        public string FilterScope { get; set; }

        public string MessageScope { get; set; } = "local";
    }
}

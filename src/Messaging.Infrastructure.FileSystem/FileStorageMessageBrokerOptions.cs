namespace Naos.Core.Messaging.Infrastructure
{
    using System.IO;
    using MediatR;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.Messaging.Domain;
    using Naos.Foundation;

    public class FileStorageMessageBrokerOptions : BaseOptions
    {
        public IMediator Mediator { get; set; }

        public IMessageHandlerFactory HandlerFactory { get; set; }

        public IFileStorage Storage { get; set; }

        public ISubscriptionMap Map { get; set; }

        public string FilterScope { get; set; }

        public string MessageScope { get; set; } = "local";

        public string Folder { get; set; } = Path.GetTempPath();

        public int ProcessDelay { get; set; } = 100;
    }
}

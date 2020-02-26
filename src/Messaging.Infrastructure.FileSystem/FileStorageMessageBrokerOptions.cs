namespace Naos.Messaging.Infrastructure
{
    using System.IO;
    using MediatR;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;
    using Naos.Messaging.Domain;
    using Naos.Tracing.Domain;

    public class FileStorageMessageBrokerOptions : OptionsBase
    {
        public ITracer Tracer { get; set; }

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

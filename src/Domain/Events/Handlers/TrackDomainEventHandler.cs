namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class TrackDomainEventHandler
        : IDomainEventHandler<IDomainEvent>
    {
        private readonly ILogger<TrackDomainEventHandler> logger;

        public TrackDomainEventHandler(ILogger<TrackDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(IDomainEvent notification)
        {
            return true;
        }

        public async Task Handle(IDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
                this.logger.LogJournal(LogEventPropertyKeys.TrackSendDomainEvent, $"{{LogKey:l}} {notification.GetType().Name.SubstringTill("DomainEvent")}", args: LogEventKeys.DomainEvent));
        }
    }
}
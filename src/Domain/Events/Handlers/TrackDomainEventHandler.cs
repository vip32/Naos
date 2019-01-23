namespace Naos.Sample.Customers.Domain
{
    using System.Collections.Generic;
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
            {
                var loggerState = new Dictionary<string, object>
                {
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
                    [LogEventPropertyKeys.TrackDomainEvent] = true
                };

                using (this.logger.BeginScope(loggerState))
                {
                    this.logger.LogInformation($"{{LogKey}} {notification.GetType().Name.SubstringTill("DomainEvent")}", LogEventKeys.DomainEvent);
                }
            });
        }
    }
}
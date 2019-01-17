namespace Naos.Core.Messaging.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class TrackMessageDomainEventHandler
        : IDomainEventHandler<MessagePublishDomainEvent>, IDomainEventHandler<MessageProcessDomainEvent>
    {
        private readonly ILogger<TrackMessageDomainEventHandler> logger;

        public TrackMessageDomainEventHandler(ILogger<TrackMessageDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(MessagePublishDomainEvent notification)
        {
            return true;
        }

        public bool CanHandle(MessageProcessDomainEvent notification)
        {
            return true;
        }

        public async Task Handle(MessagePublishDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (notification == null)
                {
                    return;
                }

                var loggerState = new Dictionary<string, object>
                {
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
                    [LogEventPropertyKeys.TrackMessage] = true
                };

                using (this.logger.BeginScope(loggerState))
                {
                    this.logger.LogInformation($"{LogEventIdentifiers.DomainEvent} {notification.GetType().Name.SubstringTill("DomainEvent")} (message={notification.Message?.GetType().PrettyName()}, id={notification.Message?.Id}, origin={notification.Message?.Origin})");
                }
            });
        }

        public async Task Handle(MessageProcessDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if(notification == null)
                {
                    return;
                }

                var loggerState = new Dictionary<string, object>
                {
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
                    [LogEventPropertyKeys.TrackMessage] = true
                };

                using (this.logger.BeginScope(loggerState))
                {
                    this.logger.LogInformation($"{LogEventIdentifiers.DomainEvent} {notification.GetType().Name.SubstringTill("DomainEvent")} (message={notification.Message?.GetType().PrettyName()}, id={notification.Message?.Id}, service={notification.MessageScope}, origin={notification.Message?.Origin})");
                }
            });
        }
    }
}
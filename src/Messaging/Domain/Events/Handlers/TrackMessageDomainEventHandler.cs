namespace Naos.Core.Messaging.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class TrackMessageDomainEventHandler
        : IDomainEventHandler<MessagePublishDomainEvent>, IDomainEventHandler<MessageHandleDomainEvent>
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

        public bool CanHandle(MessageHandleDomainEvent notification)
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

                this.logger.LogJournal(LogEventPropertyKeys.TrackPublishMessage, $"{{LogKey:l}} publish {notification.GetType().Name.SubstringTill("DomainEvent")} (message={notification.Message?.GetType().PrettyName()}, id={notification.Message?.Id}, origin={notification.Message?.Origin})", args: LogEventKeys.DomainEvent);
            });
        }

        public async Task Handle(MessageHandleDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if(notification == null)
                {
                    return;
                }

                this.logger.LogJournal(LogEventPropertyKeys.TrackHandleMessage, $"{{LogKey:l}} handle {notification.GetType().Name.SubstringTill("DomainEvent")} (message={notification.Message?.GetType().PrettyName()}, id={notification.Message?.Id}, service={notification.MessageScope}, origin={notification.Message?.Origin})", args: LogEventKeys.DomainEvent);
            });
        }
    }
}
namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class TrackDomainEventHandler
        : DomainEventHandlerBase<IDomainEvent> // handles all domainevents
    {
        protected TrackDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        /// <summary>
        /// Determines whether this instance can handle the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <returns>
        /// <c>true</c> if this instance can handle the specified notification; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanHandle(IDomainEvent notification)
        {
            return true;
        }

        public override async Task Process(IDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            this.Logger.LogJournal(LogKeys.DomainEvent, $"[{notification.EventId}] send {notification.GetType().Name.SliceTill("DomainEvent")}", LogPropertyKeys.TrackSendDomainEvent)).AnyContext();
        }
    }
}
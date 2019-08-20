namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class TrackDomainEventHandler
        : IDomainEventHandler<IDomainEvent> // handles all domainevents
    {
        private readonly ILogger<TrackDomainEventHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackDomainEventHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TrackDomainEventHandler(ILogger<TrackDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        /// Determines whether this instance can handle the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <returns>
        /// <c>true</c> if this instance can handle the specified notification; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(IDomainEvent notification)
        {
            return true;
        }

        /// <summary>
        /// Handles the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task Handle(IDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
                this.logger.LogJournal(LogKeys.DomainEvent, $"[{notification.Id}] send {notification.GetType().Name.SliceTill("DomainEvent")}", LogPropertyKeys.TrackSendDomainEvent)).AnyContext();
        }
    }
}
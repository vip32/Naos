namespace Naos.Foundation.Domain
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public abstract class DomainEventHandlerBase<TEvent> : IDomainEventHandler<TEvent>
        where TEvent : class, IDomainEvent
    {
        protected DomainEventHandlerBase(ILoggerFactory loggerFactory)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.Logger = loggerFactory.CreateLogger(this.GetType());
        }

        protected ILogger Logger { get; }

        public abstract bool CanHandle(TEvent notification);

        public virtual async Task Handle(
            TEvent notification,
            CancellationToken cancellationToken)
        {
            try
            {
                EnsureArg.IsNotNull(notification, nameof(notification));

                if (!this.CanHandle(notification))
                {
                    this.Logger.LogDebug("{LogKey:l} not processing (type={eventType}, id={eventId}, canHandle=false)", LogKeys.Domain, notification.GetType().Name, notification.EventId);
                    return;
                }

                this.Logger.LogInformation("{LogKey:l} processing (type={eventType}, id={eventId})", LogKeys.Domain, notification.GetType().Name, notification.EventId);
                var watch = Stopwatch.StartNew();

                await this.Process(notification, cancellationToken).ConfigureAwait(false);

                watch.Stop();
                this.Logger.LogInformation("{LogKey:l} processed (type={eventType}, id={eventId}) -> took {elapsed} ms", LogKeys.Domain, notification.GetType().Name, notification.EventId, watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "{LogKey:l} processing error (type={eventType}, id={eventId}): {errorMessage}", LogKeys.Domain, notification.GetType().Name, notification.EventId, ex.Message);
                throw;
            }
        }

        public abstract Task Process(
            TEvent notification,
            CancellationToken cancellationToken);
    }
}
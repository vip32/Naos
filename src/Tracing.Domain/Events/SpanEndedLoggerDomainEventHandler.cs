namespace Naos.Tracing.Domain
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;

    public class SpanEndedLoggerDomainEventHandler
        : IDomainEventHandler<SpanStartedDomainEvent>, IDomainEventHandler<SpanEndedDomainEvent>
    {
        private readonly ILogger<SpanEndedLoggerDomainEventHandler> logger;

        public SpanEndedLoggerDomainEventHandler(ILogger<SpanEndedLoggerDomainEventHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(SpanEndedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public bool CanHandle(SpanStartedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public Task Handle(SpanEndedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (notification?.Span != null)
            {
                var duration = TimeSpan.Zero;
                if(notification.Span.EndTime.HasValue && notification.Span.StartTime.HasValue)
                {
                    duration = notification.Span.EndTime.Value - notification.Span.StartTime.Value;
                }

                if (notification.Span.Status == SpanStatus.Failed)
                {
                    this.logger.LogError($"{{LogKey:l}} span failed: {notification.Span.OperationName} (id={notification.Span.SpanId}, parent={notification.Span.ParentSpanId}, kind={notification.Span.Kind}) {notification.Span.StatusDescription} -> took {duration.Humanize()}", LogKeys.Tracing);
                }
                else
                {
                    this.logger.LogDebug($"{{LogKey:l}} span finished: {notification.Span.OperationName} (id={notification.Span.SpanId}, parent={notification.Span.ParentSpanId}, kind={notification.Span.Kind}) {notification.Span.StatusDescription} -> took {duration.Humanize()}", LogKeys.Tracing);
                }
            }

            return Task.CompletedTask;
        }

        public Task Handle(SpanStartedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (notification?.Span != null)
            {
                this.logger.LogDebug($"{{LogKey:l}} span started: {notification.Span.OperationName} (id={notification.Span.SpanId}, parent={notification.Span.ParentSpanId}, kind={notification.Span.Kind}, tags={string.Join("|", notification.Span.Tags.Select(t => $"{t.Key}={t.Value}"))})", LogKeys.Tracing);
            }

            return Task.CompletedTask;
        }
    }
}

namespace Naos.Tracing.Domain
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
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
                if (notification.Span.Status == SpanStatus.Failed)
                {
                    this.logger.LogError($"{{LogKey:l}} span ended: {notification.Span.OperationName} (id={notification.Span.SpanId}, parent={notification.Span.ParentSpanId}, kind={notification.Span.Kind}) {notification.Span.StatusDescription}", LogKeys.Tracing);
                }
                else
                {
                    this.logger.LogInformation($"{{LogKey:l}} span ended: {notification.Span.OperationName} (id={notification.Span.SpanId}, parent={notification.Span.ParentSpanId}, kind={notification.Span.Kind}) {notification.Span.StatusDescription}", LogKeys.Tracing);
                }
            }

            return Task.CompletedTask;
        }

        public Task Handle(SpanStartedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (notification?.Span != null)
            {
                this.logger.LogInformation($"{{LogKey:l}} span started: {notification.Span.OperationName} (id={notification.Span.SpanId}, parent={notification.Span.ParentSpanId}, kind={notification.Span.Kind}, tags={string.Join("|", notification.Span.Tags.Select(t => $"{t.Key}={t.Value}"))})", LogKeys.Tracing);
            }

            return Task.CompletedTask;
        }
    }
}

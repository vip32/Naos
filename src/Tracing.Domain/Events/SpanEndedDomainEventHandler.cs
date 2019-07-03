namespace Naos.Core.Tracing.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class SpanEndedDomainEventHandler
        : IDomainEventHandler<SpanEndedDomainEvent>
    {
        private readonly ILogger<SpanEndedDomainEventHandler> logger;

        public SpanEndedDomainEventHandler(ILogger<SpanEndedDomainEventHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandle(SpanEndedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public Task Handle(SpanEndedDomainEvent notification, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"{{LogKey:l}} TRACING!!!!!!!!!!! {notification.Span.OperationName} {notification.Span.SpanId}", LogKeys.Tracing);
            return Task.CompletedTask;
        }
    }
}

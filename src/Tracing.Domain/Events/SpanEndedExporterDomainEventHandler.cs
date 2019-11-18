namespace Naos.Tracing.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class SpanEndedExporterDomainEventHandler
        : IDomainEventHandler<SpanStartedDomainEvent>, IDomainEventHandler<SpanEndedDomainEvent>
    {
        private readonly IEnumerable<ISpanExporter> exporters;

        public SpanEndedExporterDomainEventHandler(IEnumerable<ISpanExporter> exporters = null)
        {
            this.exporters = exporters;
        }

        public bool CanHandle(SpanEndedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public bool CanHandle(SpanStartedDomainEvent notification)
        {
            return notification?.Span != null;
        }

        public async Task Handle(SpanEndedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (this.exporters != null && notification?.Span != null)
            {
                // https://github.com/open-telemetry/opentelemetry-dotnet/blob/master/src/OpenTelemetry.Exporter.Zipkin/ZipkinTraceExporter.cs
                // TODO: queue some spans before exporting (threshold)
                foreach (var exporter in this.exporters)
                {
                    await exporter.ExportAsync(new[] { notification.Span }, cancellationToken).AnyContext();
                }
            }
        }

        public Task Handle(SpanStartedDomainEvent notification, CancellationToken cancellationToken)
        {
            //this.logger.LogInformation($"{{LogKey:l}} span started: {notification.Span.OperationName} (id={notification.Span.SpanId}, kind={notification.Span.Kind}, tags={string.Join("|", notification.Span.Tags.Select(t => $"{t.Key}={t.Value}"))})", LogKeys.Tracing);
            return Task.CompletedTask;
        }
    }
}

//namespace Naos.Tracing.Infrastructure.Jaeger
//{
//    using System.Threading;
//    using System.Threading.Tasks;
//    using Naos.Foundation;
//    using Naos.Foundation.Domain;
//    using Naos.Tracing.Domain;

//    public class SpanEndedZipkinDomainEventHandler
//        : IDomainEventHandler<SpanStartedDomainEvent>, IDomainEventHandler<SpanEndedDomainEvent>
//    {
//        private readonly ISpanExporter exporter;

//        public SpanEndedZipkinDomainEventHandler(ISpanExporter exporter)
//        {
//            this.exporter = exporter;
//        }

//        public bool CanHandle(SpanEndedDomainEvent notification)
//        {
//            return notification?.Span != null;
//        }

//        public bool CanHandle(SpanStartedDomainEvent notification)
//        {
//            return notification?.Span != null;
//        }

//        public async Task Handle(SpanEndedDomainEvent notification, CancellationToken cancellationToken)
//        {
//            if (notification?.Span != null)
//            {
//                // https://github.com/open-telemetry/opentelemetry-dotnet/blob/master/src/OpenTelemetry.Exporter.Zipkin/ZipkinTraceExporter.cs
//                // TODO: queue some spans before exporting (threshold)
//                await this.exporter.ExportAsync(new[] { notification.Span }, cancellationToken).AnyContext();
//            }
//        }

//        public Task Handle(SpanStartedDomainEvent notification, CancellationToken cancellationToken)
//        {
//            //this.logger.LogInformation($"{{LogKey:l}} span started: {notification.Span.OperationName} (id={notification.Span.SpanId}, kind={notification.Span.Kind}, tags={string.Join("|", notification.Span.Tags.Select(t => $"{t.Key}={t.Value}"))})", LogKeys.Tracing);
//            return Task.CompletedTask;
//        }
//    }
//}

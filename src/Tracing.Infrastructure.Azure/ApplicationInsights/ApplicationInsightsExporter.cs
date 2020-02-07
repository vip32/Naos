namespace Naos.Tracing.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Tracing.Domain;

    public class ApplicationInsightsExporter : ISpanExporter
    {
        // TODO: need telemetryclient
        
        public Task ExportAsync(IEnumerable<ISpan> spans, CancellationToken cancellationToken)
        {
            // TODO: implement like https://github.com/open-telemetry/opentelemetry-dotnet/blob/87d0d1eeaa077aa08e408b43ce155449b78d9e47/src/OpenTelemetry.Exporter.ApplicationInsights/ApplicationInsightsTraceExporter.cs#L36
            // HOWTO: track dependency https://www.mindbyte.nl/2019/06/28/use-application-insights-over-multiple-systems-to-track-dependencies.html
            throw new System.NotImplementedException();
        }
    }
}

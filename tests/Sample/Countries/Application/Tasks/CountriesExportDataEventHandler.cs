namespace Naos.Sample.Countries.Application
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
    using Naos.Tracing.Domain;

    public class CountriesExportDataEventHandler : QueueEventHandler<CountriesExportData>
    {
        private readonly ILogger<CountriesExportDataEventHandler> logger;
        private readonly ITracer tracer;

        public CountriesExportDataEventHandler(ILogger<CountriesExportDataEventHandler> logger, ITracer tracer)
        {
            this.logger = logger;
            this.tracer = tracer;
        }

        public override async Task<bool> Handle(QueueEvent<CountriesExportData> request, CancellationToken cancellationToken)
        {
            using (var scope = this.tracer?.BuildSpan("process countries export", LogKeys.QueueingEventHandler, SpanKind.Internal).Activate(this.logger))
            {
                await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} countries data {request.Item.Data.Timestamp:o} (id={request.Item.Id}, type={this.GetType().PrettyName()})", LogKeys.QueueingEventHandler)).AnyContext();
                throw new System.Exception("TEST");
                //return true;
            }
        }
    }
}

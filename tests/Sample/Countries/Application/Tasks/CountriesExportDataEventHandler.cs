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
        private const string LogKey = "EXPORT";
        private readonly ILogger<CountriesExportDataEventHandler> logger;
        private readonly ITracer tracer;

        public CountriesExportDataEventHandler(ILogger<CountriesExportDataEventHandler> logger, ITracer tracer)
        {
            this.logger = logger;
            this.tracer = tracer;
        }

        public override async Task<bool> Handle(QueueEvent<CountriesExportData> request, CancellationToken cancellationToken)
        {
            using (var scope = this.tracer?.BuildSpan("process export", LogKey, SpanKind.Internal).Activate(this.logger))
            {
                await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} countries data {request.Item.Data.Timestamp:o} (id={request.Item.Id}, type={this.GetType().PrettyName()})", LogKey), cancellationToken).AnyContext();

                if (RandomGenerator.GenerateInt(0, 100) % 3 == 0) // divides by 3 and checks for the remainder. A number that is divisable by 3 has no remainder (and thus ==0 and the exception will be thrown)
                {
                    throw new NaosException("Oops, the export processing randomly failed");
                }

                return true;
            }
        }
    }
}

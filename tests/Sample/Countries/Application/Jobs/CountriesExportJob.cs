namespace Naos.Sample.Countries.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.JobScheduling.Domain;
    using Naos.Queueing.Domain;
    using Naos.Sample.Countries.Domain;
    using Naos.Tracing.Domain;

    public class CountriesExportJob : Job
    {
        private readonly ILogger<CountriesImportJob> logger;
        private readonly ICountryRepository repository;
        private readonly IQueue<CountriesExportData> queue;
        private readonly ITracer tracer;

        public CountriesExportJob(ILogger<CountriesImportJob> logger, ITracer tracer, ICountryRepository repository, IQueue<CountriesExportData> queue)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = logger;
            this.tracer = tracer;
            this.repository = repository;
            this.queue = queue;
        }

        public async override Task ExecuteAsync(string correlationId, CancellationToken cancellationToken, string[] args = null)
        {
            Thread.Sleep(new TimeSpan(0, 0, 3));

            using (var scope = this.tracer.BuildSpan($"{this.GetType().Name.ToLower()} {correlationId}", LogKeys.JobScheduling, SpanKind.Consumer).Activate(this.logger))
            {
                this.logger.LogInformation("{LogKey:l} countries export", LogKeys.JobScheduling);
                var countries = await this.repository.FindAllAsync().AnyContext();
                var data = new CountriesExportData { CorrelationId = correlationId };
                using (var scope2 = this.tracer.BuildSpan("write data", LogKeys.JobScheduling).Activate(this.logger))
                {
                    this.logger.LogInformation("{LogKey:l} countries write data", LogKeys.JobScheduling);
                    // TODO: use storage to write file

                    data.Timestamp = DateTime.UtcNow;
                    data.Items = countries;

                    Thread.Sleep(new TimeSpan(0, 0, 3));
                }

                await this.queue.EnqueueAsync(data).AnyContext();
            }
        }
    }
}

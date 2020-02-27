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
        private const string LogKey = "EXPORT";
        private readonly ILogger<CountriesExportJob> logger;
        private readonly ICountryRepository repository;
        private readonly IQueue<CountriesExportData> queue;
        private readonly ITracer tracer;

        public CountriesExportJob(
            ILogger<CountriesExportJob> logger,
            ITracer tracer,
            ICountryRepository repository,
            IQueue<CountriesExportData> queue)
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

            // TODO: make this scope setup more generic > move to base Job
            using (var scope = this.tracer.BuildSpan(this.GetType().Name.ToLower(), LogKey, SpanKind.Consumer).Activate(this.logger))
            { // new scope needs to be created here as the jobscheduler span is not available here (scoping?)
                try
                {
                    this.logger.LogInformation("{LogKey:l} countries export prepare", LogKey);
                    var countries = await this.repository.FindAllAsync().AnyContext();
                    var data = new CountriesExportData { CorrelationId = correlationId };
                    using (var scope2 = this.tracer.BuildSpan("generate export").Activate(this.logger))
                    {
                        this.logger.LogInformation("{LogKey:l} countries write data", LogKey);
                        // TODO: use storage to write file

                        if (RandomGenerator.GenerateInt(0, 100) % 3 == 0) // divides by 3 and checks for the remainder. A number that is divisable by 3 has no remainder (and thus ==0 and the exception will be thrown)
                        {
                            throw new NaosException("Oops, the export generation randomly failed");
                        }

                        data.Timestamp = DateTime.UtcNow;
                        data.Items = countries;

                        Thread.Sleep(new TimeSpan(0, 0, 3));
                    }

                    await this.queue.EnqueueAsync(data).AnyContext();
                }
                catch (Exception ex)
                {
                    scope.Span.SetStatus(SpanStatus.Failed, ex.GetFullMessage());
                    throw;
                }
            }
        }
    }
}

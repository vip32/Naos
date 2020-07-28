namespace Naos.Sample.Countries.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.JobScheduling.Domain;
    using Naos.Sample.Countries.Domain;
    using Naos.Tracing.Domain;

    public class CountriesImportJob : Job
    {
        private const string LogKey = "IMPORT";
        private readonly ILogger<CountriesImportJob> logger;
        private readonly IGenericRepository<Country> repository;
        private readonly ITracer tracer;

        public CountriesImportJob(
            ILogger<CountriesImportJob> logger,
            ITracer tracer,
            IGenericRepository<Country> repository)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = logger;
            this.tracer = tracer;
            this.repository = repository;
        }

        public async override Task ExecuteAsync(string correlationId, CancellationToken cancellationToken, string[] args = null)
        {
            Thread.Sleep(new TimeSpan(0, 0, 3));

            // TODO: make this scope setup more generic > move to base Job
            using (var scope = this.tracer.BuildSpan(this.GetType().Name.ToLower(), LogKey, SpanKind.Consumer).Activate(this.logger))
            { // new scope needs to be created here as the jobscheduler span is not available here (scoping?)
                try
                {
                    this.logger.LogInformation("{LogKey:l} countries import", LogKey);
                    var countries = await this.repository.FindAllAsync().AnyContext();

                    using (var scope2 = this.tracer.BuildSpan("read data", LogKey).Activate(this.logger))
                    {
                        this.logger.LogInformation("{LogKey:l} countries read data", LogKey);
                        // TODO: use storage to read file

                        if (RandomGenerator.GenerateInt(0, 100) % 3 == 0) // divides by 3 and checks for the remainder. A number that is divisable by 3 has no remainder (and thus ==0 and the exception will be thrown)
                        {
                            throw new NaosException("Oops, the import randomly failed");
                        }

                        // TODO: send a message or so... CountriesImportedMessage

                        // TODO: foreach country in csv
                        //var code = RandomGenerator.GenerateString(2, false, true);
                        //var country = new Country { Code = code, LanguageCodes = new[] { $"{code}-{code}" }, Name = code, TenantId = "naos_sample_test", Id = code };
                        //await this.repository.UpsertAsync(country).AnyContext();

                        //code = RandomGenerator.GenerateString(2, false, true);
                        //country = new Country { Code = code, LanguageCodes = new[] { $"{code}-{code}" }, Name = code, TenantId = "naos_sample_test", Id = code };
                        //await this.repository.UpsertAsync(country).AnyContext();
                    }
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

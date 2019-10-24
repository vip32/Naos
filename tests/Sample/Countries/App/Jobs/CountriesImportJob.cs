namespace Naos.Sample.Catalogs.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.JobScheduling.Domain;
    using Naos.Sample.Countries.Domain;
    using Naos.Tracing.Domain;

    public class CountriesImportJob : Job
    {
        private readonly ILogger<CountriesImportJob> logger;
        private readonly ICountryRepository repository;
        private readonly ITracer tracer;

        public CountriesImportJob(ILogger<CountriesImportJob> logger, ITracer tracer, ICountryRepository repository)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNull(repository, nameof(repository));
            aaaaa
            this.logger = logger;
            this.tracer = tracer;
            this.repository = repository;
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken, string[] args = null)
        {
            Thread.Sleep(new TimeSpan(0, 0, 3));

            using (var scope = this.tracer.BuildSpan(this.GetType().Name.ToLower(), LogKeys.JobScheduling, SpanKind.Consumer).Activate(this.logger))
            {
                this.logger.LogInformation("{LogKey:l} countries import", LogKeys.JobScheduling);
                var countries = await this.repository.FindAllAsync().AnyContext();

                using (var scope2 = this.tracer.BuildSpan("read csv").Activate(this.logger))
                {
                    this.logger.LogInformation("{LogKey:l} countries csv read", LogKeys.JobScheduling);
                    // TODO: use storage to read file

                    // TODO: foreach country in csv
                    var code = RandomGenerator.GenerateString(2, false, true);
                    var country = new Country { Code = code, LanguageCodes = new[] { $"{code}-{code}" }, Name = code, TenantId = "naos_sample_test", Id = code };
                    await this.repository.UpsertAsync(country).AnyContext();

                    code = RandomGenerator.GenerateString(2, false, true);
                    country = new Country { Code = code, LanguageCodes = new[] { $"{code}-{code}" }, Name = code, TenantId = "naos_sample_test", Id = code };
                    await this.repository.UpsertAsync(country).AnyContext();
                }
            }
        }
    }
}

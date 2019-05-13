namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Repositories.AutoMapper;
    using Naos.Sample.Countries.Domain;
    using Naos.Sample.Countries.Infrastructure;

    [ExcludeFromCodeCoverage]
    public static partial class NaosExtensions
    {
        public static ServiceOptions AddSampleCountries(
            this ServiceOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("Countries");

            options.Context.Services.AddScoped<ICountryRepository>(sp =>
            {
                return new CountryRepository(
                    new RepositoryLoggingDecorator<Country>(
                        sp.GetRequiredService<ILogger<CountryRepository>>(),
                        new RepositoryTenantDecorator<Country>(
                            "naos_sample_test",
                            new RepositoryOrderDecorator<Country>(
                                e => e.Name,
                                new InMemoryRepository<Country, DbCountry>(o => o
                                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                    .Mediator(sp.GetRequiredService<IMediator>())
                                    .Context(sp.GetRequiredService<InMemoryContext<Country>>())
                                    .Mapper(new AutoMapperEntityMapper(MapperFactory.Create())), // singleton
                                    e => e.Identifier)))));
            });

            options.Context.Services.AddSingleton(sp => new InMemoryContext<Country>(new[]
            {
                new Country { Code = "de", LanguageCodes = new[] {"de-de" }, Name = "Germany", TenantId = "naos_sample_test", Id = "de" },
                new Country { Code = "nl", LanguageCodes = new[] {"nl-nl" }, Name = "Netherlands", TenantId = "naos_sample_test", Id = "nl" },
                new Country { Code = "be", LanguageCodes = new[] {"fr-be", "nl-be" }, Name = "Belgium", TenantId = "naos_sample_test", Id = "be" },
            }.ToList()));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: countries service added");

            return options;
        }
    }
}
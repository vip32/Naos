namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Configuration.App;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Repositories.AutoMapper;
    using Naos.Sample.Countries.Domain;
    using Naos.Sample.Countries.Infrastructure;

    public static partial class ServiceExtensions
    {
        public static ServiceOptions AddSampleCountries(
            this ServiceOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("Countries");
            options.Context.Services.AddSingleton(sp => new InMemoryContext<Country>(new[]
            {
                new Country { Code = "de", LanguageCodes = new[] {"de-de" }, Name = "Germany", TenantId = "naos_sample_test", Id = "de" },
                new Country { Code = "nl", LanguageCodes = new[] {"nl-nl" }, Name = "Netherlands", TenantId = "naos_sample_test", Id = "nl" },
                new Country { Code = "be", LanguageCodes = new[] {"fr-be", "nl-be" }, Name = "Belgium", TenantId = "naos_sample_test", Id = "be" },
            }.ToList()));

            options.Context.Services.AddScoped<ICountryRepository>(sp =>
            {
                return new CountryRepository(
                    new RepositoryLoggingDecorator<Country>(
                        sp.GetRequiredService<ILogger<CountryRepository>>(),
                        new RepositoryTenantDecorator<Country>(
                            "naos_sample_test",
                            new RepositoryOrderDecorator<Country>(
                                e => e.Name,
                                new InMemoryRepository<Country, DbCountry>(
                                    sp.GetRequiredService<ILogger<IRepository<Country>>>(),
                                    sp.GetRequiredService<IMediator>(),
                                    e => e.Identifier,
                                    sp.GetRequiredService<InMemoryContext<Country>>(), // singleton
                                    new RepositoryOptions(
                                        new AutoMapperEntityMapper(ModelMapperConfiguration.Create())),
                                    new[] { new AutoMapperSpecificationMapper<Country, DbCountry>(ModelMapperConfiguration.Create()) })))));
            });

            return options;
        }
    }
}
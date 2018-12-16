namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using System.Linq;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Repositories.AutoMapper;
    using Naos.Sample.Countries.Domain;
    using Naos.Sample.Countries.Infrastructure;

    public static partial class ServiceRegistrations
    {
        public static IServiceCollection AddSampleCountries(
            this IServiceCollection services)
        {
            services.AddSingleton(sp =>
                  new Database<DbCountry>(new[]
                                    {
                                        new DbCountry { CountryCode = "de", LanguageCodes = "de-de", CountryName = "Germany", OwnerTenant = "naos_sample_test", Identifier = "de" },
                                        new DbCountry { CountryCode = "nl", LanguageCodes = "nl-nl", CountryName = "Netherlands", OwnerTenant = "naos_sample_test", Identifier = "nl" },
                                        new DbCountry { CountryCode = "be", LanguageCodes = "fr-be;nl-be", CountryName = "Belgium", OwnerTenant = "naos_sample_test", Identifier = "be" },
                                    }.ToList()));

            services.AddScoped<ICountryRepository>(sp =>
            {
                return new CountryRepository(
                    new RepositoryLoggingDecorator<Country>(
                        sp.GetRequiredService<ILogger<CountryRepository>>(),
                        new RepositoryTenantDecorator<Country>(
                            "naos_sample_test",
                            new RepositoryOrderByDecorator<Country>(
                                e => e.Name,
                                new InMemoryRepository<Country, DbCountry>(
                                    sp.GetRequiredService<IMediator>(),
                                    e => e.Identifier,
                                    sp.GetRequiredService<Database<DbCountry>>().Entities,
                                    new RepositoryOptions(
                                        new AutoMapperEntityMapper(ModelMapperConfiguration.Create())),
                                    new[] { new AutoMapperSpecificationMapper<Country, DbCountry>(ModelMapperConfiguration.Create()) })))));
            });

            return services;
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class Database<TEntity>
#pragma warning restore SA1402 // File may only contain a single class
    {
        public Database(List<TEntity> entities)
        {
            this.Entities = entities;
        }

        public List<TEntity> Entities { get; }
    }
}
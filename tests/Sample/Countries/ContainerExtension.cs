namespace Naos.Sample.Countries
{
    using System.Linq;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Repositories.AutoMapper;
    using Naos.Sample.Countries.Domain;
    using Naos.Sample.Countries.Infrastructure;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddSampleCountries(
            this Container container)
        {
            container.RegisterSingleton<ICountryRepository>(() =>
            {
                return new CountryRepository(
                    new RepositoryLoggingDecorator<Country>(
                        container.GetInstance<ILogger<CountryRepository>>(),
                        new RepositoryTenantDecorator<Country>(
                            "naos_sample_test",
                            new InMemoryRepository<Country, CountryDto>(
                                container.GetInstance<IMediator>(),
                                e => e.Identifier,
                                new[]
                                {
                                    new CountryDto { CountryCode = "de", LanguageCodes = "de-de", CountryName = "Germany", OwnerTenant = "naos_sample_test", Identifier = "de" },
                                    new CountryDto { CountryCode = "nl", LanguageCodes = "nl-nl", CountryName = "Netherlands", OwnerTenant = "naos_sample_test", Identifier = "nl" },
                                    new CountryDto { CountryCode = "be", LanguageCodes = "fr-be;nl-be", CountryName = "Belgium", OwnerTenant = "naos_sample_test", Identifier = "be" },
                                }.AsEnumerable(),
                                new RepositoryOptions(
                                    new AutoMapperEntityMapper(ModelMapperConfiguration.Create())),
                                new[] { new AutoMapperSpecificationMapper<Country, CountryDto>(ModelMapperConfiguration.Create()) }))));
            });

            return container;
        }
    }
}
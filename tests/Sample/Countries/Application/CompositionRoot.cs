namespace Naos.Sample.Customers.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;
    using Naos.Sample.Countries.Domain;
    using Naos.Sample.Countries.Infrastructure;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static partial class CompositionRoot
    {
        public static ModuleOptions AddCountriesModule(
            this ModuleOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("countries");

            // enqueue data and do nothing === MOVED TO STARTUP ===
            //options.Context.Services.AddSingleton<IQueue<CountriesExportData>>(sp => // AddQueue<T>(sp => ....)
            //{
            //    return new InMemoryQueue<CountriesExportData>(o => o
            //        .Mediator(sp.GetService<IMediator>())
            //        .Tracer(sp.GetService<ITracer>())
            //        .LoggerFactory(sp.GetService<ILoggerFactory>())
            //        .NoRetries());
            //});
            //options.Context.Services.AddSingleton<IQueue<CountriesExportData>>(sp => // AddQueue<T>(sp => ....)
            //{
            //    return new AzureServiceBusQueue<CountriesExportData>(o => o
            //        .Mediator(sp.GetService<IMediator>())
            //        .Tracer(sp.GetService<ITracer>())
            //        .LoggerFactory(sp.GetService<ILoggerFactory>())
            //        .ConnectionString(............)
            //        .NoRetries());
            //});
            // dequeue and process data
            //options.Context.Services.AddQueueProcessItemsStartupTask<CountriesExportData>(new TimeSpan(0, 0, 30));

            options.Context.Services.AddScoped<ICountryRepository>(sp =>
            {
                return new CountryRepository(
                    new RepositoryTracingDecorator<Country>(
                        sp.GetService<ILogger<CountryRepository>>(),
                        sp.GetService<ITracer>(),
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
                                        e => e.Identifier))))));
            });

            options.Context.Services.AddSingleton(sp => new InMemoryContext<Country>(new[]
            {
                new Country { Code = "de", LanguageCodes = new[] {"de-de" }, Name = "Germany", TenantId = "naos_sample_test", Id = "de" },
                new Country { Code = "nl", LanguageCodes = new[] {"nl-nl" }, Name = "Netherlands", TenantId = "naos_sample_test", Id = "nl" },
                new Country { Code = "be", LanguageCodes = new[] {"fr-be", "nl-be" }, Name = "Belgium", TenantId = "naos_sample_test", Id = "be" },
            }.ToList()));
            options.Context.Services.AddSeederStartupTask<ICountryRepository, Country>(new[]
            {
                new Country { Code = "us", LanguageCodes = new[] {"en-us" }, Name = "United States", TenantId = "naos_sample_test", Id = "us" },
            }, delay: new TimeSpan(0, 0, 3));

            options.Context.Messages.Add($"naos services builder: countries service added");

            return options;
        }
    }
}
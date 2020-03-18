namespace Naos.Sample.Inventory.Application
{
    using System;
    using System.Linq;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Inventory.Domain;
    using Naos.Sample.Inventory.Infrastructure;
    using Naos.Tracing.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions InventoryModule(
            this ModuleOptions options,
            string section = "naos:sample:inventory")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("inventory");

            var configuration = options.Context.Configuration?.GetSection($"{section}:mongo").Get<MongoConfiguration>() ?? new MongoConfiguration();

            options.Context.Services.AddMongoClient("inventory", configuration);
            options.Context.Services.AddScoped<IInventoryRepository>(sp =>
            {
                return new InventoryRepository(
                    new RepositoryTracingDecorator<ProductInventory>(
                        sp.GetService<ILogger<InventoryRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<ProductInventory>(
                            sp.GetRequiredService<ILogger<InventoryRepository>>(),
                            new MongoRepository<ProductInventory>(o => o
                                //.Setup(sp, mongoConfiguration)
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .MongoClient(sp.GetServices<IMongoClient>()
                                    .FirstOrDefault(c => c.Settings.ApplicationName == "inventory")) //TODO: make nice extension to get a named mongoclient
                                .DatabaseName(configuration.DatabaseName)))));
            });

            options.Context.Services.AddScoped<IReplenishmentRepository>(sp =>
            {
                return new ReplenishmentRepository(
                    new RepositoryTracingDecorator<ProductReplenishment>(
                        sp.GetService<ILogger<ReplenishmentRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<ProductReplenishment>(
                            sp.GetRequiredService<ILogger<ReplenishmentRepository>>(),
                            new MongoRepository<ProductReplenishment, ProductReplenishmentDocument>(o => o
                                //.Setup(sp, mongoConfiguration)
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .MongoClient(sp.GetServices<IMongoClient>()
                                    .FirstOrDefault(c => c.Settings.ApplicationName == "inventory"))
                                .Mapper(new AutoMapperEntityMapper(MapperFactory.Create()))
                                .DatabaseName(configuration.DatabaseName)
                                .CollectionName("ProductReplenishments")))));
            });

            options.Context.Services.AddSeederStartupTask<IInventoryRepository, ProductInventory>(new[]
            {
                new ProductInventory
                {
                    Id = "548fb10e-2ad4-4bd1-9b33-6414a5ce7b10", Number = "AA1234", Quantity = 199, Region = "East"
                },
                new ProductInventory
                {
                    Id = "558fb10e-2ad4-4bd1-9b33-6414a5ce7b11", Number = "AA1234", Quantity = 188, Region = "West"
                },
                new ProductInventory
                {
                    Id = "558fb10f-2ad4-4bd1-9b33-6414a5ce7b12", Number = "BB1234", Quantity = 177, Region = "East"
                }
            }, delay: new TimeSpan(0, 0, 15));
            //options.Context.Services.AddStartupTask(sp =>
            //    new SeederStartupTask(
            //        sp.GetRequiredService<ILoggerFactory>(),
            //        sp.CreateScope().ServiceProvider.GetService(typeof(IInventoryRepository)) as IInventoryRepository));

            options.Context.Services.AddHealthChecks()
                .AddMongoDb(configuration.ConnectionString, name: "Inventory-mongodb");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: inventory service added");

            return options;
        }
    }
}
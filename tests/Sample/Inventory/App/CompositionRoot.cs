namespace Naos.Sample.Inventory.Application
{
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
        public static ModuleOptions AddInventoryModule(
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

            options.Context.Services.AddStartupTask(sp =>
                new SeederStartupTask(
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.CreateScope().ServiceProvider.GetService(typeof(IInventoryRepository)) as IInventoryRepository));

            options.Context.Services.AddHealthChecks()
                .AddMongoDb(configuration.ConnectionString, name: "Inventory-mongodb");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: inventory service added");

            return options;
        }
    }
}
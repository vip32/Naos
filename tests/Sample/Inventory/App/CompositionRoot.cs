namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Core.Events;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Inventory.Domain;
    using Naos.Tracing.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions AddInventoryModule(
            this ModuleOptions options,
            string connectionString = null,
            string section = "naos:sample:inventory:mongo")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("Inventory");

            // TODO: read configuration with conn string

            options.Context.Services.AddScoped<IInventoryRepository>(sp =>
            {
                return new InventoryRepository(
                    new RepositoryTracingDecorator<ProductInventory>(
                        sp.GetService<ILogger<InventoryRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<ProductInventory>(
                            sp.GetRequiredService<ILogger<InventoryRepository>>(),
                            new MongoDbRepository<ProductInventory>(o => o
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .Client(sp.GetRequiredService<IMongoClient>())
                                .Database("naos_sample")))));
            });

            options.Context.Services.AddSingleton<IMongoClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var mongoClientSettings = MongoClientSettings.FromUrl(
                    new MongoUrl(connectionString ?? "mongodb://localhost:27017?connectTimeoutMS=300000"));
                mongoClientSettings.ClusterConfigurator = c =>
                {
                    c.Subscribe<CommandStartedEvent>(e =>
                    {
                        logger.LogInformation($"{e.CommandName} - {e.Command.ToJson()}");
                    });
                };

                return new MongoClient(mongoClientSettings);
            });

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: inventory service added");

            return options;
        }
    }
}
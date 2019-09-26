namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
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

            options.Context.Services.AddMongoClient(connectionString);
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
                                .DatabaseName("naos_sample")))));
            });

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: inventory service added");

            return options;
        }
    }
}
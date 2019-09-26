namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Inventory.Application;
    using Naos.Sample.Inventory.Domain;
    using Naos.Tracing.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions AddInventoryModule(
            this ModuleOptions options,
            string section = "naos:sample:inventory")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("Inventory");

            var mongoConfiguration = options.Context.Configuration?.GetSection($"{section}:mongo").Get<MongoConfiguration>() ?? new MongoConfiguration();

            options.Context.Services.AddStartupTask(sp =>
                new SeederStartupTask(
                    sp.GetRequiredService<ILoggerFactory>(),
                    sp.CreateScope().ServiceProvider.GetService(typeof(IInventoryRepository)) as IInventoryRepository));

            options.Context.Services.AddMongoClient(mongoConfiguration);
            options.Context.Services.AddScoped<IInventoryRepository>(sp =>
            {
                return new InventoryRepository(
                    new RepositoryTracingDecorator<ProductInventory>(
                        sp.GetService<ILogger<InventoryRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<ProductInventory>(
                            sp.GetRequiredService<ILogger<InventoryRepository>>(),
                            new MongoDbRepository<ProductInventory>(o => o
                                //.Setup(sp, mongoConfiguration)
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .MongoClient(sp.GetRequiredService<IMongoClient>())
                                .DatabaseName(mongoConfiguration.DatabaseName)))));
            });

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: inventory service added");

            return options;
        }
    }
}
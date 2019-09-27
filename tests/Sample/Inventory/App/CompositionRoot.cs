namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Inventory.Application;
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
                            new MongoRepository<ProductInventory>(o => o
                                //.Setup(sp, mongoConfiguration)
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .MongoClient(sp.GetRequiredService<IMongoClient>())
                                .DatabaseName(mongoConfiguration.DatabaseName)))));
            });

            options.Context.Services.AddScoped<IReplenishmentRepository>(sp =>
            {
                return new ReplenishmentRepository(
                    new RepositoryTracingDecorator<ProductReplenishment>(
                        sp.GetService<ILogger<ReplenishmentRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<ProductReplenishment>(
                            sp.GetRequiredService<ILogger<ReplenishmentRepository>>(),
                            new MongoRepository<ProductReplenishment, DtoProductReplenishment>(o => o
                                //.Setup(sp, mongoConfiguration)
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .MongoClient(sp.GetRequiredService<IMongoClient>())
                                .Mapper(new AutoMapperEntityMapper(MapperFactory.Create()))
                                .DatabaseName(mongoConfiguration.DatabaseName)
                                .CollectionName("ProductReplenishments")))));
            });

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: inventory service added");

            return options;
        }
    }
}
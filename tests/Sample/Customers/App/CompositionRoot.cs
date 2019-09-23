namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure.Azure;
    using Naos.Sample.Customers.App.Client;
    using Naos.Sample.Customers.Domain;
    using Naos.Tracing.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions AddCustomersModule(
            this ModuleOptions options,
            string section = "naos:sample:customers")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("Customers");
            options.Context.AddServiceClient<UserAccountsClient>();

            var cosmosDbConfiguration = options.Context.Configuration?.GetSection($"{section}:cosmosDb").Get<CosmosDbConfiguration>();
            options.Context.Services.AddScoped<ICustomerRepository>(sp =>
            {
                return new CustomerRepository(
                    new RepositoryTracingDecorator<Customer>(
                        sp.GetService<ILogger<CustomerRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<Customer>(
                            sp.GetRequiredService<ILogger<CustomerRepository>>(),
                            new RepositoryTenantDecorator<Customer>(
                                "naos_sample_test",
                                new CosmosDbSqlRepository<Customer>(o => o
                                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                    .Mediator(sp.GetRequiredService<IMediator>())
                                    .Provider(sp.GetRequiredService<ICosmosDbSqlProvider<Customer>>())))))); // v3
                                    //.Provider(new CosmosDbSqlProviderV2<Customer>( // v2
                                    //    logger: sp.GetRequiredService<ILogger<CosmosDbSqlProviderV2<Customer>>>(),
                                    //    client: CosmosDbClientV2.Create(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken),
                                    //    databaseId: cosmosDbConfiguration.DatabaseId,
                                    //    collectionIdFactory: () => cosmosDbConfiguration.CollectionId,
                                    //    partitionKeyPath: cosmosDbConfiguration.CollectionPartitionKey,
                                    //    throughput: cosmosDbConfiguration.CollectionOfferThroughput,
                                    //    isMasterCollection: cosmosDbConfiguration.IsMasterCollection)))))));
            });

            options.Context.Services.AddScoped<ICosmosDbSqlProvider<Customer>>(sp =>
            {
                return new CosmosDbSqlProviderV3<Customer>(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .Account(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken)
                    .Database(cosmosDbConfiguration.DatabaseId)
                    .PartitionKey(e => e.Region));
            });

            var queueStorageConfiguration = options.Context.Configuration?.GetSection($"{section}:queueStorage").Get<QueueStorageConfiguration>();
            options.Context.Services.AddSingleton<IQueue<Customer>>(sp =>
            {
                var queue = new AzureStorageQueue<Customer>(
                    new AzureStorageQueueOptionsBuilder()
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .ConnectionString(queueStorageConfiguration.ConnectionString).Build());

                // for testing enqueue some items
                _ = queue.EnqueueAsync(new Customer()).Result;
                _ = queue.EnqueueAsync(new Customer()).Result;
                return queue;
            });

            options.Context.Services
                .AddHealthChecks()
                    .AddAzureQueueStorage(
                        queueStorageConfiguration.ConnectionString,
                        name: "Customers-queueStorage");

            //options.Context.Services.AddSingleton<IValidator<CreateCustomerCommand>>(new CreateCustomerCommandValidator());

            options.Context.Services
                .AddHealthChecks()
                    .AddDocumentDb(s =>
                        {
                            s.UriEndpoint = cosmosDbConfiguration.ServiceEndpointUri;
                            s.PrimaryKey = cosmosDbConfiguration.AuthKeyOrResourceToken;
                        },
                        name: "Customers-cosmosdb")
                    .AddServiceDiscoveryClient<UserAccountsClient>();

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: customers service added");

            return options;
        }
    }
}

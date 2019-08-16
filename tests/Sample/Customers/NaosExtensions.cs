namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using Naos.Core.Queueing.Infrastructure.Azure;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Customers.App.Client;
    using Naos.Sample.Customers.Domain;

    public static partial class NaosExtensions
    {
        public static ServiceOptions AddSampleCustomers(
            this ServiceOptions options,
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
                        sp.GetRequiredService<ITracer>(),
                        sp.GetRequiredService<ILogger<CustomerRepository>>(),
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
                    .PartitionKey("/Region"));
            });

            var queueStorageConfiguration = options.Context.Configuration?.GetSection($"{section}:queueStorage").Get<QueueStorageConfiguration>();
            options.Context.Services.AddSingleton<IQueue>(sp =>
            {
                var q1 = new AzureStorageQueue<Customer>(
                    new AzureStorageQueueOptionsBuilder()
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .ConnectionString(queueStorageConfiguration.ConnectionString).Build());
                _ = q1.EnqueueAsync(new Customer()).Result;
                _ = q1.EnqueueAsync(new Customer()).Result;
                return q1;
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

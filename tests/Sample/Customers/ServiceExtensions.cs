namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common.Web;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using Naos.Core.RequestCorrelation.App.Web;
    using Naos.Core.ServiceContext.App.Web;
    using Naos.Sample.Customers.App.Client;
    using Naos.Sample.Customers.Domain;

    public static partial class ServiceExtensions
    {
        public static IServiceCollection AddSampleCustomers(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:sample:customers:cosmosDb")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var cosmosDbConfiguration = configuration.GetSection(section).Get<CosmosDbConfiguration>();
            Ensure.That(cosmosDbConfiguration).IsNotNull();

            services.AddHttpClient<UserAccountsClient>()
                .AddHttpMessageHandler<HttpClientCorrelationHandler>()
                .AddHttpMessageHandler<HttpClientServiceContextHandler>()
                .AddHttpMessageHandler<HttpClientLogHandler>();

            services.AddScoped<ICustomerRepository>(sp =>
            {
                return new CustomerRepository(
                    new RepositoryLoggingDecorator<Customer>(
                        sp.GetRequiredService<ILogger<CustomerRepository>>(),
                        new RepositoryTenantDecorator<Customer>(
                            "naos_sample_test",
                            new CosmosDbSqlRepository<Customer>(
                                sp.GetRequiredService<ILogger<CustomerRepository>>(), // TODO: obsolete
                                sp.GetRequiredService<IMediator>(),
                                new CosmosDbSqlProvider<Customer>(
                                    logger: sp.GetRequiredService<ILogger<CosmosDbSqlProvider<Customer>>>(),
                                    client: CosmosDbClient.Create(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken),
                                    databaseId: cosmosDbConfiguration.DatabaseId,
                                    collectionIdFactory: () => cosmosDbConfiguration.CollectionId,
                                    collectionPartitionKey: cosmosDbConfiguration.CollectionPartitionKey,
                                    collectionOfferThroughput: cosmosDbConfiguration.CollectionOfferThroughput,
                                    isMasterCollection: cosmosDbConfiguration.IsMasterCollection)))));
            });

            services
                .AddHealthChecks()
                    .AddDocumentDb(s =>
                    {
                        s.UriEndpoint = cosmosDbConfiguration.ServiceEndpointUri;
                        s.PrimaryKey = cosmosDbConfiguration.AuthKeyOrResourceToken;
                    },
                    name: "Customers-cosmosdb")
                    .AddServiceDiscoveryClient<UserAccountsClient>();

            return services;
        }
    }
}

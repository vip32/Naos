namespace Naos.Sample.Customers
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using Naos.Sample.Customers.Domain;
    using Naos.Sample.Customers.Infrastructure;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddSampleCustomers(
            this Container container,
            IConfiguration configuration,
            string section = "naos:sample:customers:cosmosDb")
        {
            var cosmosDbConfiguration = configuration.GetSection(section).Get<CosmosDbConfiguration>();
            Ensure.That(cosmosDbConfiguration).IsNotNull();

            container.RegisterSingleton<ICustomerRepository>(() =>
                new CustomerRepositoryTenantDecorator(
                    new CustomerRepository(
                        container.GetInstance<ILogger<CustomerRepository>>(),
                        container.GetInstance<IMediator>(),
                         new CosmosDbSqlProvider<Customer>(
                            client: CosmosDbClient.Create(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken),
                            databaseId: cosmosDbConfiguration.DatabaseId,
                            collectionIdFactory: () => cosmosDbConfiguration.CollectionId,
                            collectionPartitionKey: cosmosDbConfiguration.CollectionPartitionKey,
                            collectionOfferThroughput: cosmosDbConfiguration.CollectionOfferThroughput,
                            isMasterCollection: cosmosDbConfiguration.IsMasterCollection)),
                "naos_sample_test")); // TODO: get from current context (per request instance)

            return container;
        }
    }
}

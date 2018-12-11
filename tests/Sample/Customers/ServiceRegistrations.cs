namespace Naos.Sample.Customers
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using Naos.Sample.Customers.Domain;
    using SimpleInjector;

    public static class ServiceRegistrations
    {
        public static Container AddSampleCustomers(
            this Container container,
            IConfiguration configuration,
            string section = "naos:sample:customers:cosmosDb")
        {
            var cosmosDbConfiguration = configuration.GetSection(section).Get<CosmosDbConfiguration>();
            Ensure.That(cosmosDbConfiguration).IsNotNull();

            container.RegisterSingleton<ICustomerRepository>(() =>
            {
                return new CustomerRepository(
                    new RepositoryLoggingDecorator<Customer>(
                        container.GetInstance<ILogger<CustomerRepository>>(),
                        new RepositoryTenantDecorator<Customer>(
                            "naos_sample_test",
                            new CosmosDbSqlRepository<Customer>(
                                container.GetInstance<ILogger<CustomerRepository>>(), // TODO: obsolete
                                container.GetInstance<IMediator>(),
                                new CosmosDbSqlProvider<Customer>(
                                    client: CosmosDbClient.Create(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken),
                                    databaseId: cosmosDbConfiguration.DatabaseId,
                                    collectionIdFactory: () => cosmosDbConfiguration.CollectionId,
                                    collectionPartitionKey: cosmosDbConfiguration.CollectionPartitionKey,
                                    collectionOfferThroughput: cosmosDbConfiguration.CollectionOfferThroughput,
                                    isMasterCollection: cosmosDbConfiguration.IsMasterCollection)))));
            });

            return container;
        }
    }
}

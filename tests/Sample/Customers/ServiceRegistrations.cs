namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using Naos.Sample.Customers.Domain;

    public static partial class ServiceRegistrations
    {
        public static IServiceCollection AddSampleCustomers(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:sample:customers:cosmosDb")
        {
            var cosmosDbConfiguration = configuration.GetSection(section).Get<CosmosDbConfiguration>();
            Ensure.That(cosmosDbConfiguration).IsNotNull();

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
                                    client: CosmosDbClient.Create(cosmosDbConfiguration.ServiceEndpointUri, cosmosDbConfiguration.AuthKeyOrResourceToken),
                                    databaseId: cosmosDbConfiguration.DatabaseId,
                                    collectionIdFactory: () => cosmosDbConfiguration.CollectionId,
                                    collectionPartitionKey: cosmosDbConfiguration.CollectionPartitionKey,
                                    collectionOfferThroughput: cosmosDbConfiguration.CollectionOfferThroughput,
                                    isMasterCollection: cosmosDbConfiguration.IsMasterCollection)))));
            });

            return services;
        }
    }
}

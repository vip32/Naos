namespace Naos.Sample.Customers.Infrastructure
{
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.Azure.CosmosDb;
    using Naos.Sample.Customers.Domain;

    public class CustomerRepository : CosmosDbSqlRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ILogger<CustomerRepository> logger, IMediator mediator, ICosmosDbSqlProvider<Customer> provider, IRepositoryOptions options = null)
            : base(logger, mediator, provider, options)
        {
        }
    }
}

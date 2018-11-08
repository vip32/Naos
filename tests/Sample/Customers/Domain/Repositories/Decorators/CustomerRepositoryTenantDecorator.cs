namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain.Repositories;

    public class CustomerRepositoryTenantDecorator : RepositoryTenantDecorator<Customer>, ICustomerRepository
    {
        public CustomerRepositoryTenantDecorator(ICustomerRepository decoratee, string tenantId)
            : base(decoratee, tenantId)
        {
        }
    }
}

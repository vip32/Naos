namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class CustomerRepositoryTenantDecorator : RepositorySpecificationDecorator<Customer>, ICustomerRepository
    {
        public CustomerRepositoryTenantDecorator(ICustomerRepository decoratee, string tenantId)
            : base(decoratee, new Specification<Customer>(t => t.TenantId == tenantId))
        {
        }
    }
}

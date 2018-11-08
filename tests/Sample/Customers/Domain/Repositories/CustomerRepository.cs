namespace Naos.Sample.Customers.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IRepository<Customer> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Customer> FindByNumber(string number)
            => (await this.FindAllAsync(new Specification<Customer>(e => e.CustomerNumber == number))).FirstOrDefault();
            // TODO: create proper specification + tests
    }
}

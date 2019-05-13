namespace Naos.Sample.Customers.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Repositories;

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IGenericRepository<Customer> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Customer> FindByNumber(string value)
            => (await this.FindAllAsync(new HasNumberSpecification(value))).FirstOrDefault();
    }
}

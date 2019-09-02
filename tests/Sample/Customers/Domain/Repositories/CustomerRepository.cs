namespace Naos.Sample.Customers.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IGenericRepository<Customer> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Customer> FindByNumber(string value)
            => (await this.FindAllAsync(new HasNumberSpecification(value)).AnyContext()).FirstOrDefault();
    }
}

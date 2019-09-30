namespace Naos.Sample.Customers.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(IGenericRepository<Order> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Order> FindByNumber(string value)
            => (await this.FindAllAsync(new HasOrderNumberSpecification(value)).AnyContext()).FirstOrDefault();
    }
}

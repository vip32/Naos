namespace Naos.Sample.Customers.Domain
{
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order> FindByNumber(string value);
    }
}

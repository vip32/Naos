namespace Naos.Sample.Customers.Domain
{
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer> FindByNumber(string value);
    }
}

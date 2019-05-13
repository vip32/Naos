namespace Naos.Sample.Customers.Domain
{
    using System.Threading.Tasks;
    using Naos.Core.Domain.Repositories;

    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer> FindByNumber(string value);
    }
}

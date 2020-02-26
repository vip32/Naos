namespace Naos.Sample.Catalogs.Domain
{
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product> FindOneByName(string value);
    }
}

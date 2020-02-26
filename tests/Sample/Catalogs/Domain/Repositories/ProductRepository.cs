namespace Naos.Sample.Catalogs.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(IGenericRepository<Product> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Product> FindOneByName(string value)
            => (await this.FindAllAsync(new HasNameSpecification(value)).AnyContext()).FirstOrDefault();
    }
}

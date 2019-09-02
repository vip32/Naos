namespace Naos.Sample.Countries.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CountryRepository : GenericRepository<Country>, ICountryRepository
    {
        public CountryRepository(IGenericRepository<Country> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Country> FindOneByName(string value)
            => (await this.FindAllAsync(new HasNameSpecification(value)).AnyContext()).FirstOrDefault();
    }
}

namespace Naos.Sample.Countries.Domain
{
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class CountryRepository : Repository<Country>, ICountryRepository
    {
        public CountryRepository(IRepository<Country> decoratee)
            : base(decoratee)
        {
        }

        public async Task<Country> FindByName(string name)
            => (await this.FindAllAsync(new Specification<Country>(e => e.Name == name))).FirstOrDefault();
        // TODO: create proper specification + tests
    }
}

namespace Naos.Sample.Countries.Domain
{
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    public interface ICountryRepository : IGenericRepository<Country>
    {
        Task<Country> FindOneByName(string value);
    }
}

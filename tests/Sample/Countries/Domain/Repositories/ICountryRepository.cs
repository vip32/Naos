namespace Naos.Sample.Countries.Domain
{
    using System.Threading.Tasks;
    using Naos.Core.Domain.Repositories;

    public interface ICountryRepository : IRepository<Country>
    {
        Task<Country> FindOneByName(string value);
    }
}

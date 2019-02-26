namespace Naos.Sample.Countries.App.Web.Controllers
{
    using Naos.Core.App.Web.Controllers;
    using Naos.Sample.Countries.Domain;

    public class CountriesController : NaosRepositoryControllerBase<Country, ICountryRepository>
    {
        public CountriesController(ICountryRepository repository)
            : base(repository)
        {
        }
    }
}

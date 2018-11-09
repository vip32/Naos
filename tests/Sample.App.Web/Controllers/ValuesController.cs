namespace Naos.Sample.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Sample.Countries.Domain;

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ICountryRepository repository;

        public ValuesController(ICountryRepository repository)
        {
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<Country>> Get()
        {
            return await this.repository.FindAllAsync().ConfigureAwait(false);
        }
    }
}

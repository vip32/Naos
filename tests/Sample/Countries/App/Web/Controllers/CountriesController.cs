namespace Naos.Sample.Countries.App.Web.Controllers
{
    using System.ComponentModel;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.App.Web.Controllers;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Sample.Countries.Domain;
    using NSwag.Annotations;

    public class CountriesController : NaosGenericRepositoryControllerBase<Country, ICountryRepository>
    {
        public CountriesController(ICountryRepository repository)
            : base(repository)
        {
        }

        [HttpGet]
        [Route("names/{name}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [SwaggerTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<Country>> GetByName(string name)
        {
            if(name.IsNullOrEmpty())
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            var model = await this.Repository.FindOneByName(name).AnyContext();
            if(model == null)
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            return this.Ok(model);
        }
    }
}

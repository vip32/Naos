namespace Naos.Sample.Countries.Presentation.Web
{
    using System.ComponentModel;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Application.Web;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Foundation.Domain;
    using Naos.Sample.Countries.Domain;
    using NSwag.Annotations;

    public class CountriesController : NaosGenericRepositoryControllerBase<Country, IGenericRepository<Country>>
    {
        public CountriesController(IGenericRepository<Country> repository)
            : base(repository)
        {
        }

        [HttpGet]
        [Route("names/{name}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<Country>> GetByName(string name)
        {
            if (name.IsNullOrEmpty())
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            var model = await this.Repository.FindAllAsync(new HasNameSpecification(name)).AnyContext();
            if (model == null)
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            return this.Ok(model);
        }
    }
}

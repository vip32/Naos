namespace Naos.Core.ServiceDiscovery.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web.Router;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using NSwag.Annotations;

    [Route("api/servicediscovery/router/registrations")]
    [ApiController]
    public class NaosServiceDiscoveryRegistrationsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosServiceDiscoveryRegistrationsController> logger;
        private readonly RouterContext context;

        public NaosServiceDiscoveryRegistrationsController(
            ILogger<NaosServiceDiscoveryRegistrationsController> logger,
            RouterContext context)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(context, nameof(context));

            this.logger = logger;
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos ServiceDiscovery")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<IEnumerable<ServiceRegistration>>> Get([FromQuery] string name, [FromQuery] string tag)
        {
            return this.Ok(await this.context.RegistryClient.RegistrationsAsync(name, tag).AnyContext());
        }

        [HttpPost]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos ServiceDiscovery")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<ServiceRegistration>> Post(ServiceRegistration model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new BadRequestException(this.ModelState);
            }

            var exists = (await this.context.RegistryClient.RegistrationsAsync().AnyContext()).Any(r => r.Id.Equals(model.Id, System.StringComparison.OrdinalIgnoreCase));
            await this.context.RegistryClient.RegisterAsync(model).AnyContext();
            if (exists)
            {
                return this.Accepted(this.Url.Action(nameof(this.Get), new { id = model.Id }), model);
            }
            else
            {
                return this.CreatedAtAction(nameof(this.Get), new { id = model.Id }, model);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos ServiceDiscovery")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<IActionResult> Delete(string id)
        {
            if (id.IsNullOrEmpty() || id.Equals("0", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if (!(await this.context.RegistryClient.RegistrationsAsync().AnyContext()).Any(r => r.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase)))
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            await this.context.RegistryClient.DeRegisterAsync(id).AnyContext();
            return this.NoContent();
        }
    }
}

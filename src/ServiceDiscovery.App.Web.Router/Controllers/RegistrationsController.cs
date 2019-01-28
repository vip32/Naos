namespace Naos.Core.Authentication.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.ServiceDiscovery.App;

    [Route("api/registrations")]
    [ApiController]
    public class RegistrationsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<RegistrationsController> logger;
        private readonly IServiceRegistryClient registryClient;
        private readonly IServiceRegistry registry;

        public RegistrationsController(
            ILogger<RegistrationsController> logger,
            IServiceRegistryClient registryClient,
            IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(registryClient, nameof(registryClient));
            EnsureArg.IsNotNull(registry, nameof(registry));

            this.logger = logger;
            this.registryClient = registryClient;
            this.registry = registry;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<IEnumerable<ServiceRegistration>>> Get([FromQuery] string name, [FromQuery] string tag)
        {
            return this.Ok(await this.registryClient.ServicesAsync(name, tag).ConfigureAwait(false));
        }

        [HttpPost]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<ServiceRegistration>> Post(ServiceRegistration model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new BadRequestException(this.ModelState);
            }

            bool exists = (await this.registry.RegistrationsAsync().ConfigureAwait(false)).Any(r => r.Id.Equals(model.Id));
            await this.registry.RegisterAsync(model).ConfigureAwait(false);
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
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<IActionResult> Delete(string id)
        {
            if (id.IsNullOrEmpty() || id.Equals("0"))
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if (!(await this.registry.RegistrationsAsync().ConfigureAwait(false)).Any(r => r.Id.Equals(id)))
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            await this.registry.DeRegisterAsync(id).ConfigureAwait(false);
            return this.NoContent();
        }
    }
}

namespace Naos.Core.ServiceDiscovery.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App;

    [Route("api/echo/servicediscovery")]
    [ApiController]
    public class ServiceDiscoveryEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<ServiceDiscoveryEchoController> logger;
        private readonly IServiceRegistry registry;

        public ServiceDiscoveryEchoController(
            ILogger<ServiceDiscoveryEchoController> logger,
            IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.registry = registry;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ServiceRegistration>>> Get()
        {
            return this.Ok(await this.registry.RegistrationsAsync().ConfigureAwait(false));
        }
    }
}

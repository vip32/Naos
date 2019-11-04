namespace Naos.ServiceDiscovery.Application.Web
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using NSwag.Annotations;

    [Route("api/echo/servicediscovery")]
    [ApiController]
    public class NaosServiceDiscoveryEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosServiceDiscoveryEchoController> logger;
        private readonly IServiceRegistry registry;

        public NaosServiceDiscoveryEchoController(
            ILogger<NaosServiceDiscoveryEchoController> logger,
            IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.registry = registry;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        public async Task<ActionResult<IEnumerable<ServiceRegistration>>> Get()
        {
            return this.Ok(await this.registry.RegistrationsAsync().AnyContext());
        }
    }
}

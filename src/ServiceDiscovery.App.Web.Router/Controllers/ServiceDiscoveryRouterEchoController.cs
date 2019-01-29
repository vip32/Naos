﻿namespace Naos.Core.ServiceDiscovery.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App.Web.Router;

    [Route("api/echo/router")]
    [ApiController]
    public class ServiceDiscoveryRouterEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<ServiceDiscoveryRouterEchoController> logger;
        private readonly RouterContext context;

        public ServiceDiscoveryRouterEchoController(
            ILogger<ServiceDiscoveryRouterEchoController> logger,
            RouterContext context)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(context, nameof(context));

            this.logger = logger;
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ServiceRegistration>>> Get()
        {
            return this.Ok(await this.context.RegistryClient.RegistrationsAsync().ConfigureAwait(false));
        }
    }
}

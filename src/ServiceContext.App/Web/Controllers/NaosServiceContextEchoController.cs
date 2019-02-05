namespace Naos.Core.ServiceContext.App.Web.Controllers
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    [Route("api/echo/servicecontext")]
    [ApiController]
    public class NaosServiceContextEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosServiceContextEchoController> logger;
        private readonly ServiceDescriptor serviceContext;

        public NaosServiceContextEchoController(
            ILogger<NaosServiceContextEchoController> logger,
            ServiceDescriptor serviceContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.serviceContext = serviceContext;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<ServiceDescriptor> Get()
        {
            return this.Ok(this.serviceContext);
        }
    }
}

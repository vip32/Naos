namespace Naos.Core.Correlation.App.Web.Controllers
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Correlation.App;

    [Route("api/echo/correlation")]
    [ApiController]
    public class CorrelationEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<CorrelationEchoController> logger;
        private readonly ICorrelationContextAccessor correlationContext;

        public CorrelationEchoController(
            ILogger<CorrelationEchoController> logger,
            ICorrelationContextAccessor correlationContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.correlationContext = correlationContext;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<CorrelationContext> Get()
        {
            return this.Ok(this.correlationContext.Context);
        }
    }
}

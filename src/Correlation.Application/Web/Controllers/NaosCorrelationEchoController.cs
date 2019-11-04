namespace Naos.RequestCorrelation.Application.Web
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.RequestCorrelation.Application;
    using NSwag.Annotations;

    [Route("api/echo/requestcorrelation")]
    [ApiController]
    public class NaosCorrelationEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosCorrelationEchoController> logger;
        private readonly ICorrelationContextAccessor correlationContext;

        public NaosCorrelationEchoController(
            ILogger<NaosCorrelationEchoController> logger,
            ICorrelationContextAccessor correlationContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.correlationContext = correlationContext;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        public ActionResult<CorrelationContext> Get()
        {
            return this.Ok(this.correlationContext.Context);
        }
    }
}

namespace Naos.RequestFiltering.Application.Web
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.RequestFiltering.Application;
    using NSwag.Annotations;

    [Route("naos/requestfiltering/echo")]
    [ApiController]
    public class NaosRequestFilteringEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosRequestFilteringEchoController> logger;
        private readonly IFilterContextAccessor filterContext;

        public NaosRequestFilteringEchoController(
            ILogger<NaosRequestFilteringEchoController> logger,
            IFilterContextAccessor filterContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.filterContext = filterContext;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        public ActionResult<FilterContext> Get()
        {
            return this.Ok(this.filterContext?.Context);
        }
    }
}

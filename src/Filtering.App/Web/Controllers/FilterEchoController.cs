namespace Naos.Core.Filtering.App.Web.Controllers
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Filtering.App;

    [Route("api/echo/filter")]
    [ApiController]
    public class FilterEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<FilterEchoController> logger;
        private readonly IFilterContextAccessor filterContext;

        public FilterEchoController(
            ILogger<FilterEchoController> logger,
            IFilterContextAccessor filterContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.filterContext = filterContext;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<FilterContext> Get()
        {
            return this.Ok(this.filterContext.Context);
        }
    }
}

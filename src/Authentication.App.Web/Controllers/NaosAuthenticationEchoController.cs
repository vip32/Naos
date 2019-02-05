namespace Naos.Core.Authentication.App.Web.Controllers
{
    using System.Linq;
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    [Route("api/echo/authentication")]
    [ApiController]
    public class NaosAuthenticationEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosAuthenticationEchoController> logger;

        public NaosAuthenticationEchoController(
            ILogger<NaosAuthenticationEchoController> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<object> Get()
        {
            return this.Ok(new
            {
                this.HttpContext.User?.Identity?.Name,
                this.HttpContext.User?.Identity?.IsAuthenticated,
                this.HttpContext.User?.Identity?.AuthenticationType,
                Claims = this.HttpContext.User?.Claims.Safe().Select(c => new { c.Type, c.Value })
            });
        }
    }
}

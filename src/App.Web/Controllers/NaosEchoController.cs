namespace Naos.Core.App.Web.Controllers
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Route("api/echo")]
    [ApiController]
    public class NaosEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosEchoController> logger;

        public NaosEchoController(
            ILogger<NaosEchoController> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<object> Get()
        {
            return this.Ok();
        }
    }
}

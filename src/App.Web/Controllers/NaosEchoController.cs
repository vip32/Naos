namespace Naos.App.Web.Controllers
{
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;

    [Route("api/echo")]
    [ApiController]
    public class NaosEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosEchoController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NaosEchoController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public NaosEchoController(
            ILogger<NaosEchoController> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        public ActionResult<object> Get()
        {
            return this.Ok();
        }
    }
}

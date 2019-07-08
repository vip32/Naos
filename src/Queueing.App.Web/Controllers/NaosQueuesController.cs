namespace Naos.Core.Messaging.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using NSwag.Annotations;

    [Route("api/queueing/queues")]
    [ApiController]
    public class NaosQueuesController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosQueuesController> logger;
        private readonly IEnumerable<IQueue> queues;

        public NaosQueuesController(
            ILogger<NaosQueuesController> logger,
            IEnumerable<IQueue> queues)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.queues = queues;
        }

        [HttpGet]
        [Route("{name}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Queueing")]
        public ActionResult<Dictionary<string, QueueMetrics>> Get(string name)
        {
            return this.Ok();
        }
    }
}

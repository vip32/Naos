namespace Naos.Core.Messaging.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Queueing.Domain;

    [Route("api/echo/queueing")]
    [ApiController]
    public class NaosQueueingEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosQueueingEchoController> logger;
        private readonly IEnumerable<IQueue> queues;

        public NaosQueueingEchoController(
            ILogger<NaosQueueingEchoController> logger,
            IEnumerable<IQueue> queues)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.queues = queues;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, QueueMetrics>>> Get()
        {
            var result = new Dictionary<string, QueueMetrics>();

            foreach(var queue in this.queues.Safe())
            {
                result.Add(queue.Name, await queue.GetMetricsAsync().AnyContext());
            }

            return this.Ok(result);
        }
    }
}

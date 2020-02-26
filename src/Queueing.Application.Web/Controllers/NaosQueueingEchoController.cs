namespace Naos.Messaging.Application.Web
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
    using NSwag.Annotations;

    [Route("naos/queueing/echo")]
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
        [OpenApiTag("Naos Echo")]
        public async Task<ActionResult<Dictionary<string, QueueMetrics>>> Get()
        {
            var result = new List<object>();

            foreach (var queue in this.queues.Safe())
            {
                var metrics = await queue.GetMetricsAsync().AnyContext();
                result.Add(
                    new
                    {
                        queue.Name,
                        metrics.Queued,
                        metrics.Deadlettered,
                        queue.LastEnqueuedDate,
                        queue.LastDequeuedDate,
                    });
            }

            return this.Ok(result);
        }
    }
}
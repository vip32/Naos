namespace Naos.Core.Queueing.App.Web.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.App.Web;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;
    using NSwag.Annotations;

    [Route("api/queueing/commands")]
    [ApiController]
    public class NaosQueueingCommandsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosQueueingCommandsController> logger;
        private readonly IQueue<CommandRequestWrapper> queue;

        public NaosQueueingCommandsController(
            ILogger<NaosQueueingCommandsController> logger,
            IQueue<CommandRequestWrapper> queue = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.queue = queue;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [OpenApiTag("Naos Queueing")]
        public async Task<ActionResult<QueueMetrics>> Get()
        {
            if (this.queue != null)
            {
                var metrics = await this.queue.GetMetricsAsync().AnyContext();

                return this.Ok(metrics);
            }
            else
            {
                return this.NotFound();
            }
        }
    }
}

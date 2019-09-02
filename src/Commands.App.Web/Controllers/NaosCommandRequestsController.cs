namespace Naos.Core.Commands.App.Web.Controllers
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
    public class NaosCommandRequestsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosCommandRequestsController> logger;
        private readonly IQueue<CommandWrapper> queue;

        public NaosCommandRequestsController(
            ILogger<NaosCommandRequestsController> logger,
            IQueue<CommandWrapper> queue = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.queue = queue;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [OpenApiTag("Naos Queuing")]
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

        [HttpGet]
        [Route("id")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [OpenApiTag("Naos Queuing")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ActionResult<CommandWrapper>> Get(string id)
        {
            if (this.queue != null)
            {
                // TODO: get a command and response

                return this.Ok(new CommandWrapper());
            }
            else
            {
                return this.NotFound();
            }
        }
    }
}

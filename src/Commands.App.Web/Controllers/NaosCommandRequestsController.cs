namespace Naos.Core.Commands.App.Web.Controllers
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;
    using NSwag.Annotations;

    [Route("api/commands")]
    [ApiController]
    public class NaosCommandRequestsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosCommandRequestsController> logger;
        private readonly IQueue<CommandRequestWrapper> queue;
        private readonly CommandRequestStorage commandRequestStorage;

        public NaosCommandRequestsController(
            ILogger<NaosCommandRequestsController> logger,
            IQueue<CommandRequestWrapper> queue = null,
            CommandRequestStorage commandRequestStorage = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.queue = queue;
            this.commandRequestStorage = commandRequestStorage;
        }

        [HttpGet]
        [Route("queue/metrics")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [OpenApiTag("Naos Commands")]
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
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Commands")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ActionResult<CommandRequestWrapper>> Get(string id)
        {
            if (this.commandRequestStorage != null)
            {
                try
                {
                    return this.Ok(await this.commandRequestStorage.GetAsync(id).AnyContext());
                }
                catch (FileNotFoundException)
                {
                    return this.NotFound();
                }
            }
            else
            {
                return this.NotFound();
            }
        }

        [HttpGet]
        [Route("{id}/response")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Commands")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ActionResult<object>> GetResponse(string id)
        {
            if (this.commandRequestStorage != null)
            {
                try
                {
                    return this.Ok((await this.commandRequestStorage.GetAsync(id).AnyContext()).Response);
                }
                catch (FileNotFoundException)
                {
                    return this.NotFound();
                }
            }
            else
            {
                return this.NotFound();
            }
        }
    }
}

namespace Naos.Commands.Application.Web
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
    using NSwag.Annotations;

    [Route("naos/commands")]
    [ApiController]
    public class NaosCommandRequestsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosCommandRequestsController> logger;
        private readonly IQueue<CommandRequestWrapper> queue;
        private readonly CommandRequestStore storage;

        public NaosCommandRequestsController(
            ILogger<NaosCommandRequestsController> logger,
            IQueue<CommandRequestWrapper> queue = null,
            CommandRequestStore storage = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.queue = queue;
            this.storage = storage;
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
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Commands")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ActionResult<CommandRequestWrapper>> Get(string id)
        {
            if (this.storage != null)
            {
                try
                {
                    var command = await this.storage.GetAsync(id).AnyContext();
                    if (command == null)
                    {
                        return this.NotFound();
                    }
                    else if (command.Status == CommandRequestStatus.Accepted)
                    {
                        return this.Accepted(command);
                    }
                    else if (command.Status == CommandRequestStatus.Failed)
                    {
                        this.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return new ObjectResult(command);
                    }
                    else if (command.Status == CommandRequestStatus.Cancelled)
                    {
                        this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return new ObjectResult(command);
                    }
                    else
                    {
                        return this.Ok(command);
                    }
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
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Commands")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ActionResult<object>> GetResponse(string id)
        {
            if (this.storage != null)
            {
                try
                {
                    var command = await this.storage.GetAsync(id).AnyContext();
                    if (command == null)
                    {
                        return this.NotFound();
                    }
                    else if (command.Status == CommandRequestStatus.Accepted)
                    {
                        return this.Accepted(command.Response);
                    }
                    else if (command.Status == CommandRequestStatus.Failed)
                    {
                        this.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return new ObjectResult(command.Response);
                    }
                    else if (command.Status == CommandRequestStatus.Cancelled)
                    {
                        this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return new ObjectResult(command.Response);
                    }
                    else
                    {
                        return this.Ok(command.Response);
                    }
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

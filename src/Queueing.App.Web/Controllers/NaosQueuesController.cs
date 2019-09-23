namespace Naos.Messaging.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
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

        //[HttpGet]
        //[Route("name")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[OpenApiTag("Naos Queueing")]
        //public async Task<ActionResult<QueueMetrics>> Get(string name)
        //{
        //    QueueMetrics result = null;
        //    var instances = this.HttpContext.RequestServices.GetServices(typeof(IQueue<>)); // not possible :( "Cannot create arrays of open type"

        //    foreach (var instance in instances.Safe())
        //    {
        //        var queue = instance as IQueue;
        //        if (queue.Name.SafeEquals(name))
        //        {
        //            result = await queue.GetMetricsAsync().AnyContext();
        //        }
        //    }

        //    if (result != null)
        //    {
        //        return this.Ok(result);
        //    }
        //    else
        //    {
        //        return this.NotFound();
        //    }
        //}

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Queueing")]
        public async Task<ActionResult<Dictionary<string, QueueMetrics>>> Get()
        {
            var result = new Dictionary<string, QueueMetrics>();
            var instances = this.HttpContext.RequestServices.GetServices(typeof(IQueue<>)); // not possible :( "Cannot create arrays of open type"

            foreach (var instance in instances.Safe())
            {
                var queue = instance as IQueue;
                var metrics = await queue.GetMetricsAsync().AnyContext();
                result.Add(queue.Name, metrics);
            }

            return this.Ok(result);
        }
    }
}

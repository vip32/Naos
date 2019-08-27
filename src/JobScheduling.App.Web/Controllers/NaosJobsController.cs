namespace Naos.Core.JobScheduling.App.Web
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using NSwag.Annotations;

    [Route("api/jobs")]
    [ApiController]
    public class NaosJobsController : ControllerBase
    {
        private readonly ILogger<NaosJobRegistrationsController> logger;
        private readonly IJobScheduler jobScheduler;

        public NaosJobsController(
            ILogger<NaosJobRegistrationsController> logger,
            IJobScheduler jobScheduler)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(jobScheduler, nameof(jobScheduler));

            this.logger = logger;
            this.jobScheduler = jobScheduler;
        }

        [HttpPut]
        [Route("{key}")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos JobScheduling")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult> Put(string key)
        {
            if (key.IsNullOrEmpty())
            {
                throw new BadRequestException("key cannot be empty");
            }

            var job = this.jobScheduler.Options.Registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
            if (job == null)
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            //var a = Newtonsoft.Json.JsonConvert.DeserializeObject("request content");
            // TODO: pass the content json as an object to the job
            await this.jobScheduler.TriggerAsync(key).AnyContext(); // TODO: querystring args as trigger args
            return this.Accepted();
        }
    }
}

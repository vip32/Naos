namespace Naos.Core.JobScheduling.App.Web
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.JobScheduling.Domain;

    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobRegistrationsController> logger;
        private readonly IJobScheduler jobScheduler;

        public JobsController(
            ILogger<JobRegistrationsController> logger,
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
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult> Put(string key)
        {
            if (key.IsNullOrEmpty())
            {
                throw new BadRequestException("key cannot be empty");
            }

            var model = this.jobScheduler.Settings.Registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
            if (model == null)
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            await this.jobScheduler.TriggerAsync(key).ConfigureAwait(false);
            return this.Accepted();
        }
    }
}

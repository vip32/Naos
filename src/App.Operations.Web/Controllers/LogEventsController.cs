namespace Naos.Core.App.Operations.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Operations.Domain;
    using Naos.Core.Operations.Domain.Repositories;

    [Route("api/[controller]")]
    [ApiController]
    public class LogEventsController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogEventRepository repository;

        public LogEventsController(ILogEventRepository repository)
        {
            EnsureThat.EnsureArg.IsNotNull(repository, nameof(repository));

            this.repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<LogEvent>> Get()
        {
            return await this.repository.FindAllAsync().ConfigureAwait(false);
        }

        // Application parts? https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.1
    }
}

namespace Naos.Sample.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Correlation.Domain;
    using Naos.Sample.Countries.Domain;

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> logger;
        private readonly ICountryRepository repository;
        private readonly ICorrelationContextAccessor correlationContext;

        public ValuesController(
            ILogger<ValuesController> logger,
            ICountryRepository repository,
            ICorrelationContextAccessor correlationContext)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(repository, nameof(repository));
            EnsureArg.IsNotNull(correlationContext, nameof(correlationContext));

            this.logger = logger;
            this.repository = repository;
            this.correlationContext = correlationContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Country>> Get()
        {
            this.logger.LogInformation($"hello from values controller >> {this.correlationContext.Context?.CorrelationId}");

            return await this.repository.FindAllAsync().ConfigureAwait(false);
        }
    }
}

namespace Naos.Sample.Countries.App.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Correlation;
    using Naos.Core.Common;
    using Naos.Sample.Countries.Domain;

    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ILogger<CountriesController> logger;
        private readonly ICountryRepository repository;
        private readonly ICorrelationContextAccessor correlationContext;

        public CountriesController(
            ILogger<CountriesController> logger,
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
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Country>>> Get()
        {
            this.logger.LogInformation($"hello from values controller >> {this.correlationContext.Context?.CorrelationId}");

            return this.Ok(await this.repository.FindAllAsync().ConfigureAwait(false));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Country>> Get(string id)
        {
            if (id.IsNullOrEmpty())
            {
                return this.BadRequest();
            }

            if (id.Equals("-1"))
            {
                throw new ArgumentException("-1 not allowed"); // trigger an exception to test exception handling
            }

            var result = await this.repository.FindOneAsync(id).ConfigureAwait(false);

            if(result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Country>> Update(string id, Country entity)
        {
            if (id.IsNullOrEmpty() || entity == null) // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            entity = await this.repository.UpdateAsync(entity).ConfigureAwait(false);

            return this.Accepted(this.Url.Action(nameof(this.Get), new { id = entity.Id }), entity);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Country>> Create(Country entity)
        {
            if (entity == null) // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            entity = await this.repository.UpdateAsync(entity).ConfigureAwait(false);

            return this.CreatedAtAction(nameof(this.Get), new { id = entity.Id }, entity);
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(string id)
        {
            if (id.IsNullOrEmpty())
            {
                return this.BadRequest();
            }

            await this.repository.DeleteAsync(id).ConfigureAwait(false);

            return this.NoContent();
        }
    }
}

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
    using Naos.Core.App.Web;
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
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<Country>>> Get()
        {
            this.logger.LogInformation($"hello from values controller >> {this.correlationContext.Context?.CorrelationId}");

            return this.Ok(await this.repository.FindAllAsync().ConfigureAwait(false));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Country>> Get(string id)
        {
            if (id.IsNullOrEmpty() || id.Equals("0"))
            {
                throw new BadRequestException("Model id cannot be empty");
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
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Country>> Update(string id, Country model)
        {
            if (id.IsNullOrEmpty() || id.Equals("0"))
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if (model == null) // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
            {
                throw new BadRequestException("Model cannot be empty");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            model = await this.repository.UpdateAsync(model).ConfigureAwait(false);
            return this.Accepted(this.Url.Action(nameof(this.Get), new { id = model.Id }), model);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<Country>> Create(Country model)
        {
            if (model == null) // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
            {
                throw new BadRequestException("Model cannot be empty");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            model = await this.repository.UpdateAsync(model).ConfigureAwait(false);
            return this.CreatedAtAction(nameof(this.Get), new { id = model.Id }, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            if (id.IsNullOrEmpty() || id.Equals("0"))
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            await this.repository.DeleteAsync(id).ConfigureAwait(false);
            return this.NoContent();
        }
    }
}

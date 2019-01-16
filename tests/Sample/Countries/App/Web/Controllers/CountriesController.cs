namespace Naos.Sample.Countries.App.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.RequestCorrelation.App;
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
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<IEnumerable<Country>>> Get()
        {
            this.logger.LogInformation($"hello from {this.GetType().Name} >> {this.correlationContext.Context?.CorrelationId}");

            return this.Ok(await this.repository.FindAllAsync().ConfigureAwait(false));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
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

            var model = await this.repository.FindOneAsync(id).ConfigureAwait(false);
            if(model == null)
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            return this.Ok(model);
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<Country>> Put(string id, Country model)
        {
            if (id.IsNullOrEmpty() || id.Equals("0"))
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if (!id.Equals(model.Id))
            {
                throw new BadRequestException("Model id must match route");
            }

            if (!this.ModelState.IsValid)
            {
                throw new BadRequestException(this.ModelState);
            }

            if (!await this.repository.ExistsAsync(id).ConfigureAwait(false))
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            model = await this.repository.UpdateAsync(model).ConfigureAwait(false);
            return this.Accepted(this.Url.Action(nameof(this.Get), new { id = model.Id }), model);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<ActionResult<Country>> Post(Country model)
        {
            // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
            if (!this.ModelState.IsValid)
            {
                throw new BadRequestException(this.ModelState);
            }

            if (await this.repository.ExistsAsync(model.Id).ConfigureAwait(false))
            {
                throw new BadRequestException($"Model with id {model.Id} already exists");
            }

            model = await this.repository.InsertAsync(model).ConfigureAwait(false);
            return this.CreatedAtAction(nameof(this.Get), new { id = model.Id }, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public async Task<IActionResult> Delete(string id)
        {
            if (id.IsNullOrEmpty() || id.Equals("0"))
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if (!await this.repository.ExistsAsync(id).ConfigureAwait(false))
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            await this.repository.DeleteAsync(id).ConfigureAwait(false);
            return this.NoContent();
        }
    }
}

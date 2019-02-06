namespace Naos.Sample.Countries.App.Web.Controllers
{
    using Naos.Core.App.Web.Controllers;
    using Naos.Sample.Countries.Domain;

    public class CountriesController : NaosRepositoryControllerBase<Country, ICountryRepository>
    {
        public CountriesController(ICountryRepository repository)
            : base(repository)
        {
        }

        //[HttpGet]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<IEnumerable<Country>>> Get()
        //{
        //    this.Logger.LogInformation($"+++ hello from {this.GetType().Name} >> {this.CorrelationContext?.CorrelationId}");

        //    return this.Ok(await this.Repository.FindAllAsync(
        //        this.FilterContext?.GetSpecifications<Country>(),
        //        this.FilterContext?.GetFindOptions<Country>()).AnyContext());
        //}

        //[HttpGet]
        //[Route("{id}")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<Country>> Get(string id)
        //{
        //    if (id.IsNullOrEmpty() || id.Equals("0"))
        //    {
        //        throw new BadRequestException("Model id cannot be empty");
        //    }

        //    if (id.Equals("-1"))
        //    {
        //        throw new ArgumentException("-1 not allowed"); // trigger an exception to test exception handling
        //    }

        //    var model = await this.Repository.FindOneAsync(id).AnyContext();
        //    if(model == null)
        //    {
        //        return this.NotFound(); // TODO: throw notfoundexception?
        //    }

        //    return this.Ok(model);
        //}

        //[HttpPut]
        //[Route("{id}")]
        //[ProducesResponseType((int)HttpStatusCode.Accepted)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<Country>> Put(string id, Country model)
        //{
        //    if (id.IsNullOrEmpty() || id.Equals("0"))
        //    {
        //        throw new BadRequestException("Model id cannot be empty");
        //    }

        //    if (!id.Equals(model.Id))
        //    {
        //        throw new BadRequestException("Model id must match route");
        //    }

        //    if (!this.ModelState.IsValid)
        //    {
        //        throw new BadRequestException(this.ModelState);
        //    }

        //    if (!await this.Repository.ExistsAsync(id).AnyContext())
        //    {
        //        return this.NotFound(); // TODO: throw notfoundexception?
        //    }

        //    model = await this.Repository.UpdateAsync(model).AnyContext();
        //    return this.Accepted(this.Url.Action(nameof(this.Get), new { id = model.Id }), model);
        //}

        //[HttpPost]
        //[ProducesResponseType((int)HttpStatusCode.Created)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<Country>> Post(Country model)
        //{
        //    // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
        //    if (!this.ModelState.IsValid)
        //    {
        //        throw new BadRequestException(this.ModelState);
        //    }

        //    if (await this.Repository.ExistsAsync(model.Id).AnyContext())
        //    {
        //        throw new BadRequestException($"Model with id {model.Id} already exists");
        //    }

        //    model = await this.Repository.InsertAsync(model).AnyContext();
        //    return this.CreatedAtAction(nameof(this.Get), new { id = model.Id }, model);
        //}

        //[HttpDelete]
        //[Route("{id}")]
        //[ProducesResponseType((int)HttpStatusCode.NoContent)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id.IsNullOrEmpty() || id.Equals("0"))
        //    {
        //        throw new BadRequestException("Model id cannot be empty");
        //    }

        //    if (!await this.Repository.ExistsAsync(id).AnyContext())
        //    {
        //        return this.NotFound(); // TODO: throw notfoundexception?
        //    }

        //    await this.Repository.DeleteAsync(id).AnyContext();
        //    return this.NoContent();
        //}
    }
}

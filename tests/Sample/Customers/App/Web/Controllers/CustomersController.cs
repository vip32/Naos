namespace Naos.Sample.Customers.App.Web.Controllers
{
    using EnsureThat;
    using Naos.Core.App.Web.Controllers;
    using Naos.Sample.Customers.App.Client;
    using Naos.Sample.Customers.Domain;

    public class CustomersController : NaosRepositoryControllerBase<Customer, ICustomerRepository>
    {
        private readonly UserAccountsClient userAccountsClient;

        public CustomersController(
            ICustomerRepository repository,
            UserAccountsClient userAccountsClient)
            : base(repository)
        {
            EnsureArg.IsNotNull(userAccountsClient, nameof(userAccountsClient));

            this.userAccountsClient = userAccountsClient;
        }

        //[HttpGet]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<IEnumerable<Customer>>> Get()
        //{
        //    this.Logger.LogInformation($"+++ hello from {this.GetType().Name} >> {this.CorrelationContext?.CorrelationId}");

        //    //var response = await this.userAccountsClient.HttpClient.GetAsync("api/useraccounts").AnyContext();

        //    return this.Ok(await this.Repository.FindAllAsync(
        //        this.FilterContext?.GetSpecifications<Customer>(),
        //        this.FilterContext?.GetFindOptions<Customer>()).AnyContext());
        //}

        //[HttpGet]
        //[Route("{id}")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<Customer>> Get(string id)
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
        //public async Task<ActionResult<Customer>> Put(string id, Customer model)
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
        //public async Task<ActionResult<Customer>> Post(Customer model)
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

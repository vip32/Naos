namespace Naos.Sample.UserAccounts.App.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Web.Controllers;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Sample.UserAccounts.Domain;

    public class UserAccountsController : NaosRepositoryControllerBase<UserAccount, IUserAccountRepository>
    {
        public UserAccountsController(IUserAccountRepository repository)
            : base(repository)
        {
        }

        //[HttpGet]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<IEnumerable<UserAccount>>> Get()
        //{
        //    this.Logger.LogInformation($"+++ hello from {this.GetType().Name} >> {this.CorrelationContext?.CorrelationId}");

        //    return this.Ok(await this.Repository.FindAllAsync(
        //        this.FilterContext?.GetSpecifications<UserAccount>(),
        //        this.FilterContext?.GetFindOptions<UserAccount>()).ConfigureAwait(false));
        //}

        //[HttpGet]
        //[Route("{id}")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<UserAccount>> Get(Guid id)
        //{
        //    if (id.IsDefault())
        //    {
        //        throw new BadRequestException("Model id cannot be default");
        //    }

        //    var model = await this.Repository.FindOneAsync(id).ConfigureAwait(false);
        //    if (model == null)
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
        //public async Task<ActionResult<UserAccount>> Put(Guid id, UserAccount model)
        //{
        //    if (id.IsDefault())
        //    {
        //        throw new BadRequestException("Model id cannot be default");
        //    }

        //    if (!id.Equals(model.Id))
        //    {
        //        throw new BadRequestException("Model id must match route");
        //    }

        //    if (!this.ModelState.IsValid)
        //    {
        //        throw new BadRequestException(this.ModelState);
        //    }

        //    if (!await this.Repository.ExistsAsync(id).ConfigureAwait(false))
        //    {
        //        return this.NotFound(); // TODO: throw notfoundexception?
        //    }

        //    model = await this.Repository.UpdateAsync(model).ConfigureAwait(false);
        //    return this.Accepted(this.Url.Action(nameof(this.Get), new { id = model.Id }), model);
        //}

        //[HttpPost]
        //[ProducesResponseType((int)HttpStatusCode.Created)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        //// TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        //public async Task<ActionResult<UserAccount>> Post(UserAccount model)
        //{
        //    // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
        //    if (!this.ModelState.IsValid)
        //    {
        //        throw new BadRequestException(this.ModelState);
        //    }

        //    if (await this.Repository.ExistsAsync(model.Id).ConfigureAwait(false))
        //    {
        //        throw new BadRequestException($"Model with id {model.Id} already exists");
        //    }

        //    model = await this.Repository.InsertAsync(model).ConfigureAwait(false);
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
        //    if (id.IsDefault())
        //    {
        //        throw new BadRequestException("Model id cannot be default");
        //    }

        //    if (!await this.Repository.ExistsAsync(id).ConfigureAwait(false))
        //    {
        //        return this.NotFound(); // TODO: throw notfoundexception?
        //    }

        //    await this.Repository.DeleteAsync(id).ConfigureAwait(false);
        //    return this.NoContent();
        //}
    }
}

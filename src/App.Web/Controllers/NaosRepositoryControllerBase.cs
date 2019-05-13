namespace Naos.Core.App.Web.Controllers
{
    using System.ComponentModel;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using NSwag.Annotations;

    [Route("api/[controller]")]
    public /*abstract*/ class NaosRepositoryControllerBase<TEntity, TRepo> : NaosReadOnlyRepositoryControllerBase<TEntity, TRepo>
        where TEntity : class, IEntity, IAggregateRoot
        where TRepo : class, IGenericRepository<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosRepositoryControllerBase{TEntity, TRepo}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public NaosRepositoryControllerBase(TRepo repository)
            : base(repository)
        {
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [SwaggerTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<TEntity>> Put(string id, TEntity model)
        {
            if(id.IsNullOrEmpty())
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if(!id.Equals(model.Id))
            {
                throw new BadRequestException("Model id must match route");
            }

            if(!this.ModelState.IsValid)
            {
                throw new BadRequestException(this.ModelState);
            }

            if(!await this.Repository.ExistsAsync(id).AnyContext())
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            model = await this.Repository.UpdateAsync(model).AnyContext();
            return this.Accepted(this.Url.Action(nameof(this.Get), new { id = model.Id }), model);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [SwaggerTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<TEntity>> Post(TEntity model)
        {
            // TODO: better happy path flow https://www.strathweb.com/2018/07/centralized-exception-handling-and-request-validation-in-asp-net-core/
            if(!this.ModelState.IsValid)
            {
                throw new BadRequestException(this.ModelState);
            }

            if(await this.Repository.ExistsAsync(model.Id).AnyContext())
            {
                throw new BadRequestException($"Model with id {model.Id} already exists");
            }

            model = await this.Repository.InsertAsync(model).AnyContext();
            return this.CreatedAtAction(nameof(this.Get), new { id = model.Id }, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [SwaggerTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<IActionResult> Delete(string id)
        {
            if(id.IsNullOrEmpty())
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            if(!await this.Repository.ExistsAsync(id).AnyContext())
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            await this.Repository.DeleteAsync(id).AnyContext();
            return this.NoContent();
        }
    }
}

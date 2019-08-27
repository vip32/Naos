namespace Naos.Core.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Foundation.Domain;
    using NSwag.Annotations;

    [Route("api/[controller]")]
    public /*abstract*/ class NaosReadOnlyGenericRepositoryControllerBase<TEntity, TRepository>
        : NaosControllerBase
        where TEntity : class, IEntity, IAggregateRoot
        where TRepository : class, IReadOnlyGenericRepository<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosReadOnlyGenericRepositoryControllerBase{TEntity, TRepo}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public NaosReadOnlyGenericRepositoryControllerBase(TRepository repository)
        {
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.Repository = repository;
        }

        protected TRepository Repository { get; }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> FindAll()
        {
            this.Logger.LogInformation($"+++ hello from {this.GetType().Name} >> {this.CorrelationContext?.CorrelationId}");

            return this.Ok(await this.Repository.FindAllAsync(
                this.FilterContext?.GetSpecifications<TEntity>(),
                this.FilterContext?.GetFindOptions<TEntity>()).AnyContext());
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [OpenApiTag("Naos Entity Repository")]
        [Description("TODO description")]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<TEntity>> FindOne(string id)
        {
            if (id.IsNullOrEmpty())
            {
                throw new BadRequestException("Model id cannot be empty");
            }

            //if (id.Equals("-1"))
            //{
            //    throw new ArgumentException("-1 not allowed"); // trigger an exception to test exception handling
            //}

            var model = await this.Repository.FindOneAsync(id).AnyContext();
            if (model == null)
            {
                return this.NotFound(); // TODO: throw notfoundexception?
            }

            return this.Ok(model);
        }

        [HttpGet]
        [Route("echo")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [OpenApiTag("Naos Echo")]
        [Description("TODO description")]
        public ActionResult<object> Echo()
        {
            return this.Ok();
        }
    }
}

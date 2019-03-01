namespace Naos.Core.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    public abstract class NaosReadOnlyRepositoryControllerBase<TEntity, TRepo> : NaosControllerBase
        where TEntity : class, IEntity, IAggregateRoot
        where TRepo : class, IReadOnlyRepository<TEntity>
    {
        protected NaosReadOnlyRepositoryControllerBase(TRepo repository)
        {
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.Repository = repository;
        }

        protected TRepo Repository { get; }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> Get()
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
        // TODO: use 2.2 conventions https://blogs.msdn.microsoft.com/webdev/2018/08/23/asp-net-core-2-20-preview1-open-api-analyzers-conventions/
        public virtual async Task<ActionResult<TEntity>> Get(string id)
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
    }
}

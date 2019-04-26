namespace Naos.Core.App.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    [Route("api/[controller]")]
    public /*abstract*/ class NaosEntityRepositoryControllerBase<TEntity> : NaosRepositoryControllerBase<TEntity, IRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosEntityRepositoryControllerBase{TEntity}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public NaosEntityRepositoryControllerBase(IRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

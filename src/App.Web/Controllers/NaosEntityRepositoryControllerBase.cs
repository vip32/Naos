namespace Naos.Core.App.Web.Controllers
{
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    public abstract class NaosEntityRepositoryControllerBase<TEntity> : NaosRepositoryControllerBase<TEntity, IRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosEntityRepositoryControllerBase{TEntity}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        protected NaosEntityRepositoryControllerBase(IRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

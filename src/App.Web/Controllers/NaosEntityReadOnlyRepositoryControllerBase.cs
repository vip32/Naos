namespace Naos.Core.App.Web.Controllers
{
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    public abstract class NaosEntityReadOnlyRepositoryControllerBase<TEntity> : NaosReadOnlyRepositoryControllerBase<TEntity, IReadOnlyRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosEntityReadOnlyRepositoryControllerBase{TEntity}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        protected NaosEntityReadOnlyRepositoryControllerBase(IRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

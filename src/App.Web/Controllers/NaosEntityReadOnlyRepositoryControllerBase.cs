namespace Naos.Core.App.Web.Controllers
{
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    public abstract class NaosEntityReadOnlyRepositoryControllerBase<TEntity> : NaosReadOnlyRepositoryControllerBase<TEntity, IReadOnlyRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        protected NaosEntityReadOnlyRepositoryControllerBase(IRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

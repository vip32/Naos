namespace Naos.Core.App.Web.Controllers
{
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    public abstract class NaosEntityRepositoryControllerBase<TEntity> : NaosRepositoryControllerBase<TEntity, IRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public NaosEntityRepositoryControllerBase(IRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

namespace Naos.Core.App.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;

    [Route("api/[controller]")]
    public /*abstract*/ class NaosEntityReadOnlyGenericRepositoryControllerBase<TEntity>
        : NaosReadOnlyGenericRepositoryControllerBase<TEntity, IReadOnlyGenericRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosEntityReadOnlyGenericRepositoryControllerBase{TEntity}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public NaosEntityReadOnlyGenericRepositoryControllerBase(IGenericRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

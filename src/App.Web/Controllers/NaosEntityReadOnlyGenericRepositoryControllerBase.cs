namespace Naos.App.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Naos.Foundation.Domain;

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

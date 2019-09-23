namespace Naos.App.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Naos.Foundation.Domain;

    [Route("api/[controller]")]
    public /*abstract*/ class NaosEntityGenericRepositoryControllerBase<TEntity>
        : NaosGenericRepositoryControllerBase<TEntity, IGenericRepository<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaosEntityGenericRepositoryControllerBase{TEntity}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public NaosEntityGenericRepositoryControllerBase(IGenericRepository<TEntity> repository)
            : base(repository)
        {
        }
    }
}

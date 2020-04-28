namespace Naos.Foundation.Domain
{
    using System.Threading.Tasks;

    public interface IGenericRepository<TEntity> : IReadOnlyGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        Task<TEntity> InsertAsync(TEntity entity);

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity);

        /// <summary>
        /// Delete entity by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        Task<RepositoryActionResult> DeleteAsync(object id);

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        Task<RepositoryActionResult> DeleteAsync(TEntity entity);
    }
}
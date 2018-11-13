namespace Naos.Core.Domain.Repositories
{
    using System.Threading.Tasks;

    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns></returns>
        Task<TEntity> InsertAsync(TEntity entity);

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns></returns>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns></returns>
        Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity);

        Task<ActionResult> DeleteAsync(object id);

        Task<ActionResult> DeleteAsync(TEntity entity);
    }
}
namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IReadOnlyGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IEnumerable<TEntity>> FindAllAsync(
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IEnumerable<TEntity>> FindAllAsync(
            ISpecification<TEntity> specification,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IEnumerable<TEntity>> FindAllAsync(
            IEnumerable<ISpecification<TEntity>> specifications,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds one entitiy by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        Task<TEntity> FindOneAsync(object id);

        //Task<T> FindOneAsync(ISpecification<T> specification);

        /// <summary>
        /// Entity exists by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        Task<bool> ExistsAsync(object id);
    }
}
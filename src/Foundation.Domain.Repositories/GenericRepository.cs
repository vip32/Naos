namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;

    public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IGenericRepository<TEntity> inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="inner">The decoratee.</param>
        protected GenericRepository(IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.inner = inner;
        }

        /// <summary>
        /// Delete entity by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public virtual async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            return await this.inner.DeleteAsync(id).AnyContext();
        }

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            return await this.inner.DeleteAsync(entity).AnyContext();
        }

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds one entitiy by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public virtual async Task<TEntity> FindOneAsync(object id)
        {
            return await this.inner.FindOneAsync(id).AnyContext();
        }

        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.inner.InsertAsync(entity).AnyContext();
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.inner.UpdateAsync(entity).AnyContext();
        }

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        public virtual async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.inner.UpsertAsync(entity).AnyContext();
        }

        /// <summary>
        /// Entity exists by id.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public virtual async Task<bool> ExistsAsync(object id)
        {
            return await this.inner.ExistsAsync(id).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(specification, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(IEnumerable<ISpecification<TEntity>> specifications, CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(specifications, cancellationToken).AnyContext();
        }
    }
}
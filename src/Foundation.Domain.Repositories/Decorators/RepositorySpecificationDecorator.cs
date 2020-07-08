namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IGenericRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    .-----------.        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    .------------.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositorySpecificationDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IGenericRepository<TEntity> inner;
        private readonly ISpecification<TEntity> specification;

        public RepositorySpecificationDecorator(
            ISpecification<TEntity> specification,
            IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(specification, nameof(specification));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.specification = specification;
            this.inner = inner;
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            return await this.inner.DeleteAsync(id).AnyContext();
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            return await this.inner.DeleteAsync(entity).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            var entity = await this.FindOneAsync(id).AnyContext();
            return entity != null && this.specification.IsSatisfiedBy(entity);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(
                new List<ISpecification<TEntity>>(),
                options,
                cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(
                new List<ISpecification<TEntity>>(new[] { specification }),
                options,
                cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(
                new[] { this.specification }.Concat(specifications.Safe()),
                options,
                cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            var entity = await this.inner.FindOneAsync(id).AnyContext();
            return entity != null && this.specification.IsSatisfiedBy(entity) ? entity : null;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.inner.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.inner.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.inner.UpsertAsync(entity).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await this.CountAsync(
                new List<ISpecification<TEntity>>(),
                cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await this.CountAsync(
                new List<ISpecification<TEntity>>(new[] { specification }),
                cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(IEnumerable<ISpecification<TEntity>> specifications, CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(
                new[] { this.specification }.Concat(specifications.Safe()),
                cancellationToken).AnyContext();
        }
    }
}

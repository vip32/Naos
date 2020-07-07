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
    ///    `-----------`        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    `------------`
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositoryStateSoftDeleteDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot, IStateEntity
    {
        private readonly IGenericRepository<TEntity> decoratee;
        private readonly ISpecification<TEntity> specification;

        public RepositoryStateSoftDeleteDecorator(IGenericRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.decoratee = decoratee;
            this.specification = new Specification<TEntity>(e => e.State.Deleted != true);
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return RepositoryActionResult.None;
            }

            var entity = await this.FindOneAsync(id).AnyContext();
            if (entity != null)
            {
                entity.State.SetDeleted();
                var result = (await this.UpsertAsync(entity).AnyContext()).action;
                if (result == RepositoryActionResult.Updated)
                {
                    return RepositoryActionResult.Deleted;
                }
            }

            return RepositoryActionResult.None;
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            if (entity?.Id.IsDefault() != false)
            {
                return RepositoryActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(new[] { specification }), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(
                new[] { this.specification }.Concat(specifications.Safe()),
                options,
                cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            var entity = await this.decoratee.FindOneAsync(id).AnyContext();
            return this.specification.IsSatisfiedBy(entity) ? entity : null;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await this.CountAsync(
                new List<ISpecification<TEntity>>()).AnyContext();
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
            return await this.decoratee.CountAsync(
                new[] { this.specification }.Concat(specifications.Safe()),
                cancellationToken).AnyContext();
        }
    }
}

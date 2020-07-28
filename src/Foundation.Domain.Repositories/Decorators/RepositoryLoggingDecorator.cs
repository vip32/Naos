namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

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
    public class RepositoryLoggingDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IGenericRepository<TEntity>> logger;
        private readonly IGenericRepository<TEntity> inner;
        private readonly string name;

        public RepositoryLoggingDecorator(
            ILogger<IGenericRepository<TEntity>> logger,
            IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.logger = logger;
            this.name = typeof(TEntity).PrettyName().ToLowerInvariant();
            this.inner = inner;
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            this.logger.LogInformation($"{{LogKey:l}} delete {typeof(TEntity).PrettyName()}, id: {id}", LogKeys.DomainRepository);

            return await this.inner.DeleteAsync(id).AnyContext();
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} delete {typeof(TEntity).PrettyName()}, id: {entity?.Id}", LogKeys.DomainRepository);

            return await this.inner.DeleteAsync(entity).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            this.logger.LogInformation($"{{LogKey:l}} exists {typeof(TEntity).PrettyName()}, id: {id}", LogKeys.DomainRepository);

            return await this.inner.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} findall {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.inner.FindAllAsync(options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} findall {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            if (specification != null)
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} specification: {specification}");
            }

            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.inner.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} findall {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            foreach (var specification in specifications.Safe())
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} specification: {specification}");
            }

            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.inner.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            this.logger.LogInformation($"{{LogKey:l}} findone {typeof(TEntity).PrettyName()}, id: {id}", LogKeys.DomainRepository);

            return await this.inner.FindOneAsync(id).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} insert {typeof(TEntity).PrettyName()}, id: {entity?.Id}", LogKeys.DomainRepository);

            return await this.inner.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} update {typeof(TEntity).PrettyName()}, id: {entity?.Id}", LogKeys.DomainRepository);

            return await this.inner.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} upsert {typeof(TEntity).PrettyName()}, id: {entity?.Id}", LogKeys.DomainRepository);

            return await this.inner.UpsertAsync(entity).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} count {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            return await this.inner.CountAsync(cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} count {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            return await this.inner.CountAsync(specification, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(IEnumerable<ISpecification<TEntity>> specifications, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} count {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            return await this.inner.CountAsync(specifications, cancellationToken).AnyContext();
        }
    }
}

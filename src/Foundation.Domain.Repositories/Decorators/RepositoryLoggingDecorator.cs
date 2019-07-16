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
        private readonly IGenericRepository<TEntity> decoratee;
        private readonly string name;

        public RepositoryLoggingDecorator(
            ILogger<IGenericRepository<TEntity>> logger,
            IGenericRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.logger = logger;
            this.decoratee = decoratee;
            this.name = typeof(TEntity).PrettyName().ToLowerInvariant();
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            this.logger.LogInformation($"{{LogKey:l}} delete {typeof(TEntity).PrettyName()}, id: {id}", LogKeys.DomainRepository);

            return await this.decoratee.DeleteAsync(id).AnyContext();
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} delete {typeof(TEntity).PrettyName()}, id: {entity?.Id}", LogKeys.DomainRepository);

            return await this.decoratee.DeleteAsync(entity).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            this.logger.LogInformation($"{{LogKey:l}} exists {typeof(TEntity).PrettyName()}, id: {id}", LogKeys.DomainRepository);

            return await this.decoratee.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} findall {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            foreach(var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.decoratee.FindAllAsync(options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} findall {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            if(specification != null)
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} specification: {specification.ToString()}");
            }

            foreach(var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.decoratee.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} findall {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            foreach(var specification in specifications.Safe())
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} specification: {specification.ToString()}");
            }

            foreach(var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            this.logger.LogInformation($"{{LogKey:l}} findone {typeof(TEntity).PrettyName()}, id: {id}", LogKeys.DomainRepository);

            return await this.decoratee.FindOneAsync(id).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} insert {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            return await this.decoratee.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            this.logger.LogInformation($"{{LogKey:l}} update {typeof(TEntity).PrettyName()}", LogKeys.DomainRepository);

            return await this.decoratee.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            var result = await this.decoratee.UpsertAsync(entity).AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} upserted {result.GetType().PrettyName()}, id: {result.entity.Id}, action: {result.action.ToDescription()}", LogKeys.DomainRepository);
            return result;
        }
    }
}

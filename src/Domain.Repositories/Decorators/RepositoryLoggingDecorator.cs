namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    public class RepositoryLoggingDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IRepository<TEntity>> logger;
        private readonly IRepository<TEntity> decoratee;

        public RepositoryLoggingDecorator(ILogger<IRepository<TEntity>> logger, IRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.logger = logger;
            this.decoratee = decoratee;
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            return await this.decoratee.DeleteAsync(id).AnyContext();
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            return await this.decoratee.DeleteAsync(entity).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            foreach(var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogEventKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.decoratee.FindAllAsync(options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if(specification != null)
            {
                this.logger.LogDebug($"{LogEventKeys.DomainRepository} specification: {specification.ToString()}");
            }

            foreach(var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogEventKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.decoratee.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            foreach(var specification in specifications.Safe())
            {
                this.logger.LogDebug($"{LogEventKeys.DomainRepository} specification: {specification.ToString()}");
            }

            foreach(var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                this.logger.LogDebug($"{LogEventKeys.DomainRepository} order: {order.Expression.ToExpressionString()}");
            }

            return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            var result = await this.decoratee.UpsertAsync(entity).AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} upserted entity: {result.GetType().PrettyName()}, id: {result.entity.Id}, action: {result.action.ToDescription()}", LogEventKeys.DomainRepository);
            return result;
        }
    }
}

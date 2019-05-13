namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class CosmosDbSqlRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot //, IDiscriminated
    {
        private readonly ILogger<IGenericRepository<TEntity>> logger;
        private readonly CosmosDbSqlRepositoryOptions<TEntity> options;

        public CosmosDbSqlRepository(CosmosDbSqlRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));

            this.options = options;
            this.logger = options.CreateLogger<IGenericRepository<TEntity>>();
        }

        public CosmosDbSqlRepository(Builder<CosmosDbSqlRepositoryOptionsBuilder<TEntity>, CosmosDbSqlRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new CosmosDbSqlRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby
            var entities = await this.options.Provider
                .WhereAsync(
                    count: options?.Take ?? -1, // TODO: implement cosmosdb skip/take https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-sql-query#OffsetLimitClause
                    orderExpression: order?.Expression,
                    orderDescending: order?.Direction == OrderDirection.Descending).AnyContext();
            return entities.ToList();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby
            var entities = await this.options.Provider
                .WhereAsync(
                    expression: specification?.ToExpression().Expand(), // expand fixes Invoke in expression
                    count: options?.Take ?? -1, // TODO: implement cosmosdb skip/take https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-sql-query#OffsetLimitClause
                    orderExpression: order?.Expression,
                    orderDescending: order?.Direction == OrderDirection.Descending).AnyContext();
            return entities.ToList();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby
            var entities = await this.options.Provider
                .WhereAsync(
                    expressions: specifications.Safe().Select(s => s.ToExpression().Expand()), // expand fixes Invoke in expression
                    count: options?.Take ?? -1, // TODO: implement cosmosdb skip/take https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-sql-query#OffsetLimitClause
                    orderExpression: order?.Expression,
                    orderDescending: order?.Direction == OrderDirection.Descending).AnyContext();
            return entities.ToList();
        }

        public async Task<TEntity> FindOneAsync(object id) // partitionkey
        {
            if(id.IsDefault())
            {
                return default;
            }

            return await this.options.Provider.GetByIdAsync(id as string);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if(id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id) != null;
        }

        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns></returns>
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns></returns>
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns></returns>
        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if(entity == null)
            {
                return (default, ActionResult.None);
            }

            var isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).AnyContext();

            if(this.options.PublishEvents && this.options.Mediator != null)
            {
                if(isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
                }
            }

            if(isNew)
            {
                if(entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }
            }
            else if(entity is IStateEntity stateEntity)
            {
                stateEntity.State.SetUpdated();
            }

            this.logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            var result = await this.options.Provider.UpsertAsync(entity).AnyContext();
            //entity = result;

            if(this.options.PublishEvents && this.options.Mediator != null)
            {
                if(isNew)
                {
                    //await this.mediator.Publish(new EntityInsertedDomainEvent<IEntity>(result)).AnyContext();
                    await this.options.Mediator.Publish(new EntityInsertedDomainEvent(result)).AnyContext();
                }
                else
                {
                    //await this.mediator.Publish(new EntityUpdatedDomainEvent<IEntity>(result)).AnyContext();
                    await this.options.Mediator.Publish(new EntityUpdatedDomainEvent(result)).AnyContext();
                }
            }

            //this.logger.LogInformation($"{{LogKey:l}} upserted entity: {result.GetType().PrettyName()}, id: {result.Id}, isNew: {isNew}", LogEventKeys.DomainRepository);
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isNew ? (result, ActionResult.Inserted) : (result, ActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            if(id.IsDefault())
            {
                return ActionResult.None;
            }

            var entity = await this.FindOneAsync(id).AnyContext();
            if(entity != null)
            {
                if(this.options.PublishEvents && this.options.Mediator != null)
                {
                    await this.options.Mediator.Publish(new EntityDeleteDomainEvent(entity)).AnyContext();
                }

                this.logger.LogInformation($"{{LogKey:l}} delete entity: {entity.GetType().PrettyName()}, id: {entity.Id}", LogKeys.DomainRepository);
                await this.options.Provider.DeleteByIdAsync(id as string).AnyContext();

                if(this.options.PublishEvents && this.options.Mediator != null)
                {
                    await this.options.Mediator.Publish(new EntityDeletedDomainEvent(entity)).AnyContext();
                }

                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if(entity?.Id.IsDefault() != false)
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }
    }
}
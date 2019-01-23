namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class CosmosDbSqlRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot //, IDiscriminated
    {
        private readonly ILogger<IRepository<TEntity>> logger;
        private readonly IMediator mediator;
        private readonly ICosmosDbSqlProvider<TEntity> provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlRepository{T}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="options">The options.</param>
        public CosmosDbSqlRepository(
            ILogger<IRepository<TEntity>> logger,
            IMediator mediator,
            ICosmosDbSqlProvider<TEntity> provider,
            IRepositoryOptions options = null)
        {
            EnsureArg.IsNotNull(logger, nameof(mediator));
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(provider, nameof(provider));

            this.logger = logger;
            this.mediator = mediator;
            this.provider = provider;
            this.Options = options;
        }

        protected IRepositoryOptions Options { get; }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby
            var entities = await this.provider
                .WhereAsync(
                    count: options?.Take ?? -1, // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
                    orderExpression: order?.Expression,
                    orderDescending: order?.Direction == OrderDirection.Descending).ConfigureAwait(false);
            // TODO: implement orderby (options)
            return entities.ToList();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby
            var entities = await this.provider
                .WhereAsync(
                    expression: specification?.ToExpression().Expand(), // expand fixes Invoke in expression
                    count: options?.Take ?? -1, // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
                    orderExpression: order?.Expression,
                    orderDescending: order?.Direction == OrderDirection.Descending).ConfigureAwait(false);
            // TODO: implement orderby (options)
            return entities.ToList();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby
            var entities = await this.provider
                .WhereAsync(
                    expressions: specifications.Safe().Select(s => s.ToExpression().Expand()), // expand fixes Invoke in expression
                    count: options?.Take ?? -1, // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
                    orderExpression: order?.Expression,
                    orderDescending: order?.Direction == OrderDirection.Descending).ConfigureAwait(false);
            return entities.ToList();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return default;
            }

            return await this.provider.GetByIdAsync(id as string);
            //return await this.provider.FirstOrDefaultAsync(o => o.Id == id).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
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
            var result = await this.UpsertAsync(entity).ConfigureAwait(false);
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns></returns>
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).ConfigureAwait(false);
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns></returns>
        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (default, ActionResult.None);
            }

            bool isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).ConfigureAwait(false);

            if (this.Options?.PublishEvents != false)
            {
                if (isNew)
                {
                    await this.mediator.Publish(new EntityInsertDomainEvent(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdateDomainEvent(entity)).ConfigureAwait(false);
                }
            }

            this.logger.LogInformation($"{{LogKey}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogEventKeys.DomainRepository);
            var result = await this.provider.UpsertAsync(entity).ConfigureAwait(false);
            entity = result;

            if (this.Options?.PublishEvents != false)
            {
                if (isNew)
                {
                    //await this.mediator.Publish(new EntityInsertedDomainEvent<IEntity>(result)).ConfigureAwait(false);
                    await this.mediator.Publish(new EntityInsertedDomainEvent(result)).ConfigureAwait(false);
                }
                else
                {
                    //await this.mediator.Publish(new EntityUpdatedDomainEvent<IEntity>(result)).ConfigureAwait(false);
                    await this.mediator.Publish(new EntityUpdatedDomainEvent(result)).ConfigureAwait(false);
                }
            }

            this.logger.LogInformation($"{{LogKey}} upserted entity: {result.GetType().PrettyName()}, id: {result.Id}, isNew: {isNew}", LogEventKeys.DomainRepository);
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isNew ? (result, ActionResult.Inserted) : (result, ActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return ActionResult.None;
            }

            var entity = await this.FindOneAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeleteDomainEvent(entity)).ConfigureAwait(false);
                }

                this.logger.LogInformation($"{{LogKey}} delete entity: {entity.GetType().PrettyName()}, id: {entity.Id}", LogEventKeys.DomainRepository);
                await this.provider.DeleteByIdAsync(id as string).ConfigureAwait(false);

                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeletedDomainEvent(entity)).ConfigureAwait(false);
                }

                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if (entity?.Id.IsDefault() != false)
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
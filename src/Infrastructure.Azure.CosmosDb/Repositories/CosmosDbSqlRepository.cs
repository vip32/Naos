namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class CosmosDbSqlRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IMediator mediator;
        private readonly ICosmosDbSqlProvider<TEntity> provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlRepository{T}" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="options">The options.</param>
        public CosmosDbSqlRepository(IMediator mediator, ICosmosDbSqlProvider<TEntity> provider, IRepositoryOptions options = null)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(provider, nameof(provider));

            this.mediator = mediator;
            this.provider = provider;
            this.Options = options;
        }

        protected IRepositoryOptions Options { get; }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            return await this.provider.WhereAsync<TEntity>(maxItemCount: options?.Take ?? -1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            return await this.provider.WhereAsync<TEntity>(
                expression: specification?.ToExpression().Expand(),
                maxItemCount: options?.Take ?? -1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();

            return await this.provider.WhereAsync<TEntity>(
                expressions: specificationsArray.NullToEmpty().Select(s => s.ToExpression().Expand()),
                maxItemCount: options?.Take ?? -1).ConfigureAwait(false);
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await this.provider.FirstOrDefaultAsync(o => o.Id == id).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id) != null;
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            bool isTransient = entity.Id.IsDefault();

            if (isTransient && entity is IStateEntity)
            {
                // TODO: do this in an notification handler (EntityAddDomainEvent)
                entity.As<IStateEntity>().State.CreatedDate = new DateTimeEpoch();
            }

            if (!isTransient && entity is IStateEntity)
            {
                // TODO: do this in an notification handler (EntityUpdateDomainEvent)
                entity.As<IStateEntity>().State.UpdatedDate = new DateTimeEpoch();
            }

            if (this.Options?.PublishEvents != false)
            {
                if (isTransient)
                {
                    await this.mediator.Publish(new EntityCreateDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdateDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
            }

            var result = await this.provider.AddOrUpdateAsync(entity).ConfigureAwait(false);

            if (this.Options?.PublishEvents != false)
            {
                if (isTransient)
                {
                    await this.mediator.Publish(new EntityCreatedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
            }

            return result;
        }

        public async Task DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return;
            }

            var entity = await this.FindOneAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeleteDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }

                await this.provider.DeleteAsync(id as string).ConfigureAwait(false);

                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
            }
        }

        public async Task DeleteAsync(TEntity entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return;
            }

            await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
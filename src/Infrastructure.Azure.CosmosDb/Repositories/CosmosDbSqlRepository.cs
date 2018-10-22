namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CosmosDbSqlRepository<T> : IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        private readonly IMediator mediator;
        private readonly ICosmosDbSqlProvider<T> provider;

        public CosmosDbSqlRepository(IMediator mediator, ICosmosDbSqlProvider<T> provider)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(provider, nameof(provider));

            this.mediator = mediator;
            this.provider = provider;
        }

        public async Task<IEnumerable<T>> FindAllAsync(IFindOptions options = null)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            return await this.provider.WhereAsync<T>(maxItemCount: options?.Take ?? -1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, IFindOptions options = null)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            return await this.provider.WhereAsync<T>(
                expression: specification?.ToExpression().Expand(),
                maxItemCount: options?.Take ?? -1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, IFindOptions options = null)
        {
            // TODO: implement cosmosdb skip/take once available https://feedback.azure.com/forums/263030-azure-cosmos-db/suggestions/6350987--documentdb-allow-paging-skip-take
            var specificationsArray = specifications as ISpecification<T>[] ?? specifications.ToArray();

            return await this.provider.WhereAsync<T>(
                expressions: specificationsArray.NullToEmpty().Select(s => s.ToExpression().Expand()),
                maxItemCount: options?.Take ?? -1).ConfigureAwait(false);
        }

        public async Task<T> FindOneAsync(object id)
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

        public async Task<T> AddOrUpdateAsync(T entity)
        {
            if (entity == null)
            {
                return null;
            }

            bool isNew = entity.Id.IsDefault();

            var result = await this.provider.AddOrUpdateAsync(entity).ConfigureAwait(false);

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<T>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<T>(entity)).ConfigureAwait(false);
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
                await this.provider.DeleteAsync(id as string).ConfigureAwait(false);
                await this.mediator.Publish(new EntityDeletedDomainEvent<T>(entity)).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return;
            }

            await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
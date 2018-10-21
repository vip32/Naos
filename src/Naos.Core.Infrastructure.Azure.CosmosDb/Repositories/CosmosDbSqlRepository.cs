namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CosmosDbSqlRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IMediator mediator;
        private readonly ICosmosDbSqlProvider<TEntity> provider;

        public CosmosDbSqlRepository(IMediator mediator, ICosmosDbSqlProvider<TEntity> provider)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(provider, nameof(provider));

            this.mediator = mediator;
            this.provider = provider;
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(int count = -1)
        {
            return await this.provider.WhereAsync<TEntity>(maxItemCount: count).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, int count = -1)
        {
            return await this.provider.WhereAsync<TEntity>(
                expression: specification?.ToExpression(),
                maxItemCount: count).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            //EnsureArg.IsNotNull(specificationsArray);
            //EnsureArg.IsTrue(specificationsArray.Any());

            var expressions = specificationsArray.NullToEmpty().Select(s => s.ToExpression());
            return await this.provider.WhereAsync<TEntity>(
                expressions: expressions,
                maxItemCount: count).ConfigureAwait(false);
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

            bool isNew = entity.Id.IsDefault();

            var result = await this.provider.AddOrUpdateAsync(entity).ConfigureAwait(false);

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
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
                await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
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
namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CosmosDbSqlRepository<TEntity> : IRepository<TEntity, string>
        where TEntity : Entity<string>, IAggregateRoot
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
            EnsureArg.IsNotNull(specification);

            return await this.provider.WhereAsync<TEntity>(
                expression: specification.Expression(),
                maxItemCount: count).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            EnsureArg.IsNotNull(specificationsArray);
            EnsureArg.IsTrue(specificationsArray.Any());

            var expressions = specificationsArray.Select(s => s.Expression());
            return await this.provider.WhereAsync<TEntity>(
                expressions: expressions,
                maxItemCount: count).ConfigureAwait(false);
        }

        public async Task<TEntity> FindByIdAsync(string id)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            return await this.provider.FirstOrDefaultAsync(o => o.Id == id).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await this.FindByIdAsync(id) != null;
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            bool isNew = entity.Id.IsNullOrEmpty();

            var result = await this.provider.AddOrUpdateAsync(entity).ConfigureAwait(false);

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<TEntity, string>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity, string>(entity)).ConfigureAwait(false);
            }

            return result;
        }

        public async Task DeleteAsync(string id)
        {
            EnsureArg.IsNotNullOrEmpty(id);

            var entity = await this.FindByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                await this.provider.DeleteAsync(id).ConfigureAwait(false);
                await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity, string>(entity)).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);
            EnsureArg.IsNotNullOrEmpty(entity.Id);

            await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
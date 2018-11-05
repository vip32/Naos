namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    public class RepositoryTenantDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
    {
        private readonly IRepository<TEntity> decoratee;
        private readonly string tenantId;

        public RepositoryTenantDecorator(IRepository<TEntity> decoratee, string tenantId)
        {
            EnsureThat.EnsureArg.IsNotNull(decoratee);

            this.decoratee = decoratee;
            this.tenantId = tenantId;
        }

        public async Task DeleteAsync(object id)
        {
            await this.decoratee.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await this.decoratee.DeleteAsync(entity).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(), options).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(new[] { specification }), options).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            return await this.decoratee.FindAllAsync(
                new[] { HasTenantSpecification<TEntity>.Factory.Create(this.tenantId) }.Concat(specifications.NullToEmpty()),
                options).ConfigureAwait(false);
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).ConfigureAwait(false);
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).ConfigureAwait(false);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).ConfigureAwait(false);
        }

        public async Task<(TEntity entity, UpsertAction action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).ConfigureAwait(false);
        }
    }
}

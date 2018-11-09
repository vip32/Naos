namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Domain.Specifications;

    public class RepositoryOrderByDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly Expression<Func<TEntity, object>> orderBy;
        private readonly IRepository<TEntity> decoratee;

        public RepositoryOrderByDecorator(
            Expression<Func<TEntity, object>> orderBy,
            IRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(orderBy, nameof(orderBy));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.orderBy = orderBy;
            this.decoratee = decoratee;
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
            options = this.EnsureOptions(options);
            return await this.decoratee.FindAllAsync(options).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            options = this.EnsureOptions(options);
            return await this.decoratee.FindAllAsync(specification, options).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            options = this.EnsureOptions(options);
            return await this.decoratee.FindAllAsync(specifications, options).ConfigureAwait(false);
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

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).ConfigureAwait(false);
        }

        private IFindOptions<TEntity> EnsureOptions(IFindOptions<TEntity> options)
        {
            if (options == null)
            {
                options = new FindOptions<TEntity>();
            }

            options.OrderBy = this.orderBy;
            return options;
        }
    }
}

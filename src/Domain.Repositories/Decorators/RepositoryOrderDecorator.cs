namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Domain.Specifications;

    public class RepositoryOrderDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly Expression<Func<TEntity, object>> orderExpression;
        private readonly OrderDirection orderDirection;
        private readonly IRepository<TEntity> decoratee;

        public RepositoryOrderDecorator(
            Expression<Func<TEntity, object>> orderByExpression, // TODO: accept a proper OrderByOption collection
            IRepository<TEntity> decoratee)
            : this(orderByExpression, OrderDirection.Ascending, decoratee)
        {
        }

        public RepositoryOrderDecorator(
            Expression<Func<TEntity, object>> orderExpression, // TODO: accept a proper OrderByOption collection
            OrderDirection orderDirection,
            IRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(orderExpression, nameof(orderExpression));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.orderExpression = orderExpression;
            this.orderDirection = orderDirection;
            this.decoratee = decoratee;
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            return await this.decoratee.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            return await this.decoratee.DeleteAsync(entity).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            options = this.EnsureOptions(options);
            return await this.decoratee.FindAllAsync(options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            options = this.EnsureOptions(options);
            return await this.decoratee.FindAllAsync(specification, options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            options = this.EnsureOptions(options);
            return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).ConfigureAwait(false);
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

            options.Order = new OrderOption<TEntity>(this.orderExpression, this.orderDirection);
            return options;
        }
    }
}

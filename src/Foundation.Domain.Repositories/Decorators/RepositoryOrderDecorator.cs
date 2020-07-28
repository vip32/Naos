namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IGenericRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    .-----------.        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    .------------.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositoryOrderDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly Expression<Func<TEntity, object>> orderExpression;
        private readonly OrderDirection orderDirection;
        private readonly IGenericRepository<TEntity> inner;

        public RepositoryOrderDecorator(
            Expression<Func<TEntity, object>> orderByExpression, // TODO: accept a proper OrderByOption collection
            IGenericRepository<TEntity> inner)
            : this(orderByExpression, OrderDirection.Ascending, inner)
        {
        }

        public RepositoryOrderDecorator(
            Expression<Func<TEntity, object>> orderExpression, // TODO: accept a proper OrderByOption collection
            OrderDirection orderDirection,
            IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(orderExpression, nameof(orderExpression));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.orderExpression = orderExpression;
            this.orderDirection = orderDirection;
            this.inner = inner;
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            return await this.inner.DeleteAsync(id).AnyContext();
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            return await this.inner.DeleteAsync(entity).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.inner.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            options = this.EnsureOptions(options);
            return await this.inner.FindAllAsync(options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            options = this.EnsureOptions(options);
            return await this.inner.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            options = this.EnsureOptions(options);
            return await this.inner.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.inner.FindOneAsync(id).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.inner.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.inner.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.inner.UpsertAsync(entity).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(specification, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(IEnumerable<ISpecification<TEntity>> specifications, CancellationToken cancellationToken = default)
        {
            return await this.inner.CountAsync(specifications, cancellationToken).AnyContext();
        }

        private IFindOptions<TEntity> EnsureOptions(IFindOptions<TEntity> options)
        {
            if (options == null)
            {
                options = new FindOptions<TEntity>();
            }

            options.Orders = options.Orders.Insert(new OrderOption<TEntity>(this.expression, this.direction));
            return options;
        }
    }
}

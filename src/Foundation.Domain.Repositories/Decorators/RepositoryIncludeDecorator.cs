namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    public class RepositoryIncludeDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IEnumerable<Expression<Func<TEntity, object>>> expressions;
        private readonly IGenericRepository<TEntity> inner;

        public RepositoryIncludeDecorator(
            Expression<Func<TEntity, object>> expression,
            IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(expression, nameof(expression));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.expressions = new[] { expression };
            this.inner = inner;
        }

        public RepositoryIncludeDecorator(
            IEnumerable<Expression<Func<TEntity, object>>> expressions,
            IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(expressions, nameof(expressions));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.expressions = expressions;
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

            options.Includes = options.Includes.InsertRange(
                this.expressions.Select(e => new IncludeOption<TEntity>(e)));

            return options;
        }
    }
}

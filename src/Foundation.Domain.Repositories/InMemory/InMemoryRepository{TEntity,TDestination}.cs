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
    /// Represents an InMemoryRepository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the domain entity.</typeparam>
    /// <typeparam name="TDestination">The type of the destination/remote dto.</typeparam>
    /// <seealso cref="Domain.InMemoryRepository{T}" />
    public class InMemoryRepository<TEntity, TDestination> : InMemoryRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly Func<TDestination, object> idSelector;

        public InMemoryRepository(
            InMemoryRepositoryOptions<TEntity> options,
            Func<TDestination, object> idSelector)
            : base(options)
        {
            EnsureArg.IsNotNull(idSelector, nameof(idSelector));

            this.idSelector = idSelector; // TODO: really needed?
        }

        public InMemoryRepository(
            Builder<InMemoryRepositoryOptionsBuilder<TEntity>,
                InMemoryRepositoryOptions<TEntity>> optionsBuilder,
            Func<TDestination, object> idSelector)
            : this(optionsBuilder(new InMemoryRepositoryOptionsBuilder<TEntity>()).Build(), idSelector)
        {
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentOutOfRangeException">id.</exception>
        public override async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return default;
            }

            var result = this.Options.Context.Entities.Safe().Select(e => this.Options.Mapper.Map<TDestination>(e)) // work on destination objects
                .SingleOrDefault(e => this.idSelector(e).Equals(id)); // TODO: use HasIdSpecification + MapExpression (makes idSelector obsolete)
            // return (await this.FindAllAsync(new HasIdSpecification<TEntity>(id))).FirstOrDefault();

            if (this.Options.Mapper != null && result != null)
            {
                return await Task.FromResult(this.Options.Mapper.Map<TEntity>(result)).AnyContext();
            }

            return default;
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        public override async Task<IEnumerable<TEntity>> FindAllAsync(
            IEnumerable<ISpecification<TEntity>> specifications,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            var result = this.Options.Context.Entities.Safe().Select(e => this.Options.Mapper.Map<TDestination>(e)); // work on destination objects

            foreach (var specification in specifications.Safe())
            {
                result = result.Where(this.EnsurePredicate(specification)); // translate specification to destination predicate
            }

            return await Task.FromResult(this.FindAll(result, options)).AnyContext();
        }

        protected new Func<TDestination, bool> EnsurePredicate(ISpecification<TEntity> specification)
        {
            return this.Options.Mapper.MapSpecification<TEntity, TDestination>(specification);
        }

        protected IEnumerable<TEntity> FindAll(IEnumerable<TDestination> entities, IFindOptions<TEntity> options = null)
        {
            var result = entities;

            if (options?.Skip.HasValue == true && options.Skip.Value > 0)
            {
                result = result.Skip(options.Skip.Value);
            }

            if (options?.Take.HasValue == true && options.Take.Value > 0)
            {
                result = result.Take(options.Take.Value);
            }

            IOrderedEnumerable<TDestination> orderedResult = null;
            foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
            {
                orderedResult = orderedResult == null
                    ? order.Direction == OrderDirection.Ascending
                        ? result.OrderBy(this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression).Compile())
                        : result.OrderByDescending(this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression).Compile())
                    : order.Direction == OrderDirection.Ascending // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
                        ? orderedResult.ThenBy(this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression).Compile())
                        : orderedResult.ThenByDescending(this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(order.Expression).Compile());
            }

            if (orderedResult != null)
            {
                result = orderedResult;
            }

            if (this.Options.Mapper != null && result != null)
            {
                return result.Select(d => this.Options.Mapper.Map<TEntity>(d));
            }

            return null;
        }
    }
}
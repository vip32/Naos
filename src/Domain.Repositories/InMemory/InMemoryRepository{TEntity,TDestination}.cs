namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// Represents an InMemoryRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the domain entity</typeparam>
    /// <typeparam name="TDestination">The type of the destination/remote dto.</typeparam>
    /// <seealso cref="Domain.InMemoryRepository{T}" />
    public class InMemoryRepository<TEntity, TDestination> : InMemoryRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IEnumerable<ISpecificationMapper<TEntity, TDestination>> specificationMappers;
        private readonly Func<TDestination, object> idSelector;

        public InMemoryRepository(
            ILogger<IRepository<TEntity>> logger,
            IMediator mediator,
            Func<TDestination, object> idSelector,
            InMemoryContext<TEntity> context,
            IRepositoryOptions options = null,
            IEnumerable<ISpecificationMapper<TEntity, TDestination>> specificationMappers = null)
            : base(logger, mediator, context, options)
        {
            EnsureArg.IsNotNull(idSelector, nameof(idSelector));
            EnsureArg.IsNotNull(options?.Mapper, nameof(options.Mapper));

            //base.entities = entities.NullToEmpty().Select(d => this.Options.Mapper.Map<TEntity>(d));
            this.specificationMappers = specificationMappers;
            this.idSelector = idSelector;
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns></returns>
        public override async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var result = this.context.Entities.Safe().Select(d => this.Options.Mapper.Map<TDestination>(d)); // work on destination objects

            foreach (var specification in specifications.Safe())
            {
                result = result.Where(this.EnsurePredicate(specification)); // translate specification to destination predicate
            }

            return await Task.FromResult(this.FindAll(result, options));
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public override async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return default;
            }

            var result = this.context.Entities.Safe().Select(d => this.Options.Mapper.Map<TDestination>(d)) // work on destination objects
                .SingleOrDefault(e => this.idSelector(e).Equals(id)); // TODO: use HasIdSpecification + MapExpression (makes idSelector obsolete)
            // return (await this.FindAllAsync(new HasIdSpecification<TEntity>(id))).FirstOrDefault();

            if (this.Options?.Mapper != null && result != null)
            {
                return await Task.FromResult(this.Options.Mapper.Map<TEntity>(result));
            }

            return default;
        }

        protected new Func<TDestination, bool> EnsurePredicate(ISpecification<TEntity> specification)
        {
            foreach(var specificationMapper in this.specificationMappers.Safe())
            {
                if (specificationMapper.CanHandle(specification))
                {
                    return specificationMapper.Map(specification);
                }
            }

            throw new NaosException($"no applicable specification mapper found for {specification.GetType().PrettyName()}");
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

            if (this.Options?.Mapper != null && result != null)
            {
                return result.Select(d => this.Options.Mapper.Map<TEntity>(d));
            }

            return null;
        }
    }
}
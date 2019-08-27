namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents an InMemoryRepository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the domain entity.</typeparam>
    /// <seealso cref="Domain.IRepository{T, TId}" />
    public class InMemoryRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();

        public InMemoryRepository(InMemoryRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));

            this.Options = options;
            this.Logger = options.CreateLogger<IGenericRepository<TEntity>>();
            this.Options.Context ??= new InMemoryContext<TEntity>();
            this.Options.IdGenerator ??= new InMemoryEntityIdGenerator<TEntity>(this.Options.Context);
        }

        public InMemoryRepository(Builder<InMemoryRepositoryOptionsBuilder<TEntity>, InMemoryRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new InMemoryRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        protected InMemoryRepositoryOptions<TEntity> Options { get; }

        protected ILogger<IGenericRepository<TEntity>> Logger { get; set; }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        public async Task<IEnumerable<TEntity>> FindAllAsync(
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(specifications: null, options: options, cancellationToken: cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// /// <param name="cancellationToken">The cancellationToken.</param>
        public async Task<IEnumerable<TEntity>> FindAllAsync(
            ISpecification<TEntity> specification,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            return specification == null
                ? await this.FindAllAsync(specifications: null, options: options).AnyContext()
                : await this.FindAllAsync(new[] { specification }, options, cancellationToken).AnyContext();
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// /// <param name="cancellationToken">The cancellationToken.</param>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(
            IEnumerable<ISpecification<TEntity>> specifications,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            var result = this.Options.Context.Entities.AsEnumerable();

            foreach (var specification in specifications.Safe())
            {
                result = result.Where(this.EnsurePredicate(specification));
            }

            return await Task.FromResult(this.FindAll(result, options, cancellationToken).ToList()).AnyContext();
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentOutOfRangeException">id.</exception>
        public virtual async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return default;
            }

            this.@lock.EnterReadLock();
            try
            {
                var result = this.Options.Context.Entities.FirstOrDefault(x => x.Id.Equals(id));

                if (this.Options.Mapper != null && result != null)
                {
                    return this.Options.Mapper.Map<TEntity>(result);
                }

                return await Task.FromResult(result).AnyContext();
            }
            finally
            {
                this.@lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Asynchronous checks if element exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public virtual async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id).AnyContext() != null;
        }

        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (default, ActionResult.None);
            }

            var isTransient = entity.Id.IsDefault();
            var isNew = isTransient || !await this.ExistsAsync(entity.Id).AnyContext();

            if (isTransient)
            {
                this.Options.IdGenerator.SetNew(entity);
            }

            if (this.Options.PublishEvents)
            {
                if (isNew)
                {
                    await this.Options.Mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.Options.Mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
                }
            }

            if (isNew)
            {
                if (entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }
            }
            else if (entity is IStateEntity stateEntity)
            {
                stateEntity.State.SetUpdated();
            }

            this.Logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            // TODO: map to destination
            //this.entities = this.entities.Where(e => !e.Id.Equals(entity.Id)).Concat(new[] { entity }).ToList();
            this.@lock.EnterWriteLock();
            try
            {
                if (this.Options.Context.Entities.Contains(entity))
                {
                    this.Options.Context.Entities.Remove(entity);
                }

                this.Options.Context.Entities.Add(entity);
            }
            finally
            {
                this.@lock.ExitWriteLock();
            }

            if (this.Options.PublishEvents)
            {
                if (isNew)
                {
                    await this.Options.Mediator.Publish(new EntityInsertedDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.Options.Mediator.Publish(new EntityUpdatedDomainEvent(entity)).AnyContext();
                }
            }

            //this.logger.LogInformation($"{{LogKey:l}} upserted entity: {entity.GetType().PrettyName()}, id: {entity.Id}, isNew: {isNew}", LogEventKeys.DomainRepository);
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isNew ? (entity, ActionResult.Inserted) : (entity, ActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentOutOfRangeException">id.</exception>
        public async Task<ActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return ActionResult.None;
            }

            var entity = this.Options.Context.Entities.FirstOrDefault(x => x.Id.Equals(id));
            if (entity != null)
            {
                if (this.Options.PublishEvents)
                {
                    await this.Options.Mediator.Publish(new EntityDeleteDomainEvent(entity)).AnyContext();
                }

                this.Logger.LogInformation($"{{LogKey:l}} delete entity: {entity.GetType().PrettyName()}, id: {entity.Id}", LogKeys.DomainRepository);
                this.@lock.EnterWriteLock();
                try
                {
                    this.Options.Context.Entities.Remove(entity);
                }
                finally
                {
                    this.@lock.ExitWriteLock();
                }

                if (this.Options.PublishEvents)
                {
                    await this.Options.Mediator.Publish(new EntityDeletedDomainEvent(entity)).AnyContext();
                }

                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Id.</exception>
        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if (entity?.Id.IsDefault() != false)
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }

        protected virtual Func<TEntity, bool> EnsurePredicate(ISpecification<TEntity> specification)
        {
            return specification.ToPredicate();
        }

        protected virtual IEnumerable<TEntity> FindAll(IEnumerable<TEntity> entities, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            this.@lock.EnterReadLock();

            try
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

                IOrderedEnumerable<TEntity> orderedResult = null;
                foreach (var order in (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order))
                {
                    orderedResult = orderedResult == null
                        ? order.Direction == OrderDirection.Ascending
                            ? result.OrderBy(order.Expression.Compile()) // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
                            : result.OrderByDescending(order.Expression.Compile())
                        : order.Direction == OrderDirection.Ascending
                            ? orderedResult.ThenBy(order.Expression.Compile())
                            : orderedResult.ThenByDescending(order.Expression.Compile());
                }

                if (orderedResult != null)
                {
                    result = orderedResult;
                }

                if (this.Options.Mapper != null && result != null)
                {
                    return result.Select(r => this.Options.Mapper.Map<TEntity>(r));
                }

                return result;
            }
            finally
            {
                this.@lock.ExitReadLock();
            }
        }
    }
}
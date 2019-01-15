namespace Naos.Core.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// Represents an InMemoryRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the domain entity</typeparam>
    /// <seealso cref="Domain.IRepository{T, TId}" />
    public class InMemoryRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        protected readonly InMemoryContext<TEntity> context;
        private readonly IMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryRepository{T}" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="context">The context containing entities.</param>
        /// <param name="options">The options.</param>
        public InMemoryRepository(
            IMediator mediator,
            InMemoryContext<TEntity> context,
            IRepositoryOptions options = null)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(context, nameof(context));

            this.mediator = mediator;
            this.context = context ?? new InMemoryContext<TEntity>();
            this.Options = options;
        }

        protected IRepositoryOptions Options { get; }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            return await this.FindAllAsync(specifications: null, options: options).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            return specification == null
                ? await this.FindAllAsync(specifications: null, options: options).ConfigureAwait(false)
                : await this.FindAllAsync(new[] { specification }, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            var result = this.context.Entities.AsEnumerable();

            foreach (var specification in specifications.NullToEmpty())
            {
                result = result.Where(this.EnsurePredicate(specification));
            }

            return await Task.FromResult(this.FindAll(result, options).ToList()).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public virtual async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return default;
            }

            var result = this.context.Entities.FirstOrDefault(x => x.Id.Equals(id));

            if (this.Options?.Mapper != null && result != null)
            {
                return this.Options.Mapper.Map<TEntity>(result);
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Asynchronous checks if element exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id) != null;
        }

        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns></returns>
        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).ConfigureAwait(false);
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns></returns>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).ConfigureAwait(false);
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns></returns>
        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (default, ActionResult.None);
            }

            bool isTransient = entity.Id.IsDefault();
            bool isNew = isTransient || !await this.ExistsAsync(entity.Id).ConfigureAwait(false);

            if (isTransient)
            {
                this.EnsureId(entity);
            }

            if (this.Options?.PublishEvents != false)
            {
                if (isNew)
                {
                    await this.mediator.Publish(new EntityInsertDomainEvent(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdateDomainEvent(entity)).ConfigureAwait(false);
                }
            }

            // TODO: map to destination
            //this.entities = this.entities.Where(e => !e.Id.Equals(entity.Id)).Concat(new[] { entity }).ToList();
            if (this.context.Entities.Contains(entity))
            {
                this.context.Entities.Remove(entity);
            }

            this.context.Entities.Add(entity);

            if (this.Options?.PublishEvents != false)
            {
                if (isNew)
                {
                    await this.mediator.Publish(new EntityInsertedDomainEvent(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdatedDomainEvent(entity)).ConfigureAwait(false);
                }
            }

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isNew ? (entity, ActionResult.Inserted) : (entity, ActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public async Task<ActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return ActionResult.None;
            }

            var entity = this.context.Entities.FirstOrDefault(x => x.Id.Equals(id));
            if (entity != null)
            {
                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeleteDomainEvent(entity)).ConfigureAwait(false);
                }

                this.context.Entities.Remove(entity);

                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeletedDomainEvent(entity)).ConfigureAwait(false);
                }

                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Id</exception>
        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if (entity?.Id.IsDefault() != false)
            {
                return ActionResult.None;
            }

            if (this.Options?.PublishEvents != false)
            {
                await this.mediator.Publish(new EntityDeleteDomainEvent(entity)).ConfigureAwait(false);
            }

            this.context.Entities.Remove(entity);

            if (this.Options?.PublishEvents != false)
            {
                await this.mediator.Publish(new EntityDeletedDomainEvent(entity)).ConfigureAwait(false);
            }

            return ActionResult.Deleted; // TODO: check if something actually got delete
        }

        protected virtual Func<TEntity, bool> EnsurePredicate(ISpecification<TEntity> specification)
        {
            return specification.ToPredicate();
        }

        protected virtual IEnumerable<TEntity> FindAll(IEnumerable<TEntity> entities, IFindOptions<TEntity> options = null)
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

            foreach (var orderBy in options?.OrderBy.NullToEmpty())
            {
                if (orderBy.Direction == OrderByDirection.Ascending)
                {
                    result = result.OrderBy(orderBy.Expression.Compile());
                }
                else
                {
                    result = result.OrderByDescending(orderBy.Expression.Compile());
                }
            }

            if (this.Options?.Mapper != null && result != null)
            {
                return result.Select(r => this.Options.Mapper.Map<TEntity>(r));
            }

            return result;
        }

        private void EnsureId(TEntity entity) // TODO: move this to seperate class (IdentityGenerator)
        {
            if (entity is IEntity<int>)
            {
                (entity as IEntity<int>).Id = this.context.Entities.Count() + 1;
            }
            else if (entity is IEntity<string>)
            {
                (entity as IEntity<string>).Id = Guid.NewGuid().ToString();
            }
            else if (entity is IEntity<Guid>)
            {
                (entity as IEntity<Guid>).Id = Guid.NewGuid();
            }
            else
            {
                throw new NotSupportedException($"entity id type {entity.Id.GetType().Name} not supported");
                // TODO: or just set Id to null?
            }
        }
    }
}
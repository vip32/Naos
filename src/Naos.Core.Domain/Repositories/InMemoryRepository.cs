namespace Naos.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;

    /// <summary>
    /// Represents an InMemoryRepository
    /// </summary>
    /// <typeparam name="TEntity">Type or the Entity stored</typeparam>
    /// <seealso cref="Domain.IRepository{TEntity, TId}" />
    public class InMemoryRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        protected IEnumerable<TEntity> entities;
        private readonly IMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="idFactory">Method that generates a new Id if needed</param>
        public InMemoryRepository(IMediator mediator, IEnumerable<TEntity> entities = null)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.mediator = mediator;
            this.entities = entities.NullToEmpty();
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync(int count = -1)
        {
            return await Task.FromResult(this.entities).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, int count = -1)
        {
            if(specification == null)
            {
                return await this.FindAllAsync(count).ConfigureAwait(false);
            }

            return await Task.FromResult(this.entities.Where(specification.ToPredicate())).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1)
        {
            var specsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var result = this.entities;

            foreach (var specification in specsArray.NullToEmpty())
            {
                result = result.Where(specification.ToPredicate());
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public async Task<TEntity> FindAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await Task.FromResult(this.entities.FirstOrDefault(x => x.Id.Equals(id)));
        }

        /// <summary>
        /// Asynchronous checks if element exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindAsync(id) != null;
        }

        /// <summary>
        /// Adds or updates asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Method for generating new Ids not provided</exception>
        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            bool isNew = false;
            if (entity.Id.IsDefault())
            {
                if (entity is IEntity<int>)
                {
                    (entity as IEntity<int>).Id = this.entities.Count() + 1;
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
                    throw new NotSupportedException($"Entity Id type {entity.Id.GetType().Name}");
                    // TODO: or just set Id to null?
                }

                isNew = true;
            }

            this.entities = this.entities.Concat(new[] { entity }.AsEnumerable());

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }

            return entity;
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public async Task DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return;
            }

            var entity = this.entities.FirstOrDefault(x => x.Id.Equals(id));
            if (entity != null)
            {
                this.entities = this.entities.Where(x => !x.Id.Equals(entity.Id));
                await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Id</exception>
        public async Task DeleteAsync(TEntity entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return;
            }

            this.entities = this.entities.Where(x => !x.Id.Equals(entity.Id));
            await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
        }
    }
}
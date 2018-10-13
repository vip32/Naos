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
    /// <typeparam name="TId">The type of the Entity identifier.</typeparam>
    /// <seealso cref="Domain.IRepository{TEntity, TId}" />
    public class InMemoryRepository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : Entity<TId>, IAggregateRoot
    {
        protected IEnumerable<TEntity> entities;
        private readonly IMediator mediator;
        private readonly Func<IEnumerable<TEntity>, TId> idFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryRepository{T, TId}"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="idFactory">Method that generates a new Id if needed</param>
        public InMemoryRepository(IMediator mediator, IEnumerable<TEntity> entities = null, Func<IEnumerable<TEntity>, TId> idFactory = null)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.mediator = mediator;
            this.entities = entities.NullToEmpty();
            this.idFactory = idFactory;
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
            EnsureArg.IsNotNull(specification);

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
            EnsureArg.IsNotNull(specsArray);
            EnsureArg.IsTrue(specsArray.Length > 0);

            var result = this.entities;

            foreach (var specification in specsArray)
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
        public async Task<TEntity> FindByIdAsync(TId id)
        {
            if (id.IsDefault())
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            return await Task.FromResult(this.entities.FirstOrDefault(x => x.Id.Equals(id)));
        }

        /// <summary>
        /// Asynchronous checks if element exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(TId id)
        {
            return await this.FindByIdAsync(id) != null;
        }

        /// <summary>
        /// Adds or updates asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Method for generating new Ids not provided</exception>
        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);

            bool isNew = false;
            if (entity.Id.IsDefault())
            {
                if (this.idFactory == null)
                {
                    throw new Exception("id generator not available");
                }

                entity.Id = this.idFactory(this.entities);
                isNew = true;
            }

            this.entities = this.entities.Concat(new[] { entity }.AsEnumerable());

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<TEntity, TId>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity, TId>(entity)).ConfigureAwait(false);
            }

            return entity;
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public async Task DeleteAsync(TId id)
        {
            if (id.IsDefault())
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            var entity = this.entities.FirstOrDefault(x => x.Id.Equals(id));
            if (entity != null)
            {
                this.entities = this.entities.Where(x => !x.Id.Equals(entity.Id));
                await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity, TId>(entity)).ConfigureAwait(false);
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
            EnsureArg.IsNotNull(entity);
            if (entity.Id.IsDefault())
            {
                throw new ArgumentOutOfRangeException(nameof(entity.Id));
            }

            this.entities = this.entities.Where(x => !x.Id.Equals(entity.Id));
            await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity, TId>(entity)).ConfigureAwait(false);
        }
    }
}
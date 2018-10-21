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
    /// <typeparam name="T">Type or the Entity stored</typeparam>
    /// <seealso cref="Domain.IRepository{T, TId}" />
    public class InMemoryRepository<T> : IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        protected IEnumerable<T> entities;
        private readonly IMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryRepository{T}"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="idFactory">Method that generates a new Id if needed</param>
        public InMemoryRepository(IMediator mediator, IEnumerable<T> entities = null)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.mediator = mediator;
            this.entities = entities.NullToEmpty();
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAllAsync(int? skip = null, int? take = null)
        {
            var result = this.entities;

            if (skip.HasValue && skip.Value > 0)
            {
                result = result.Skip(skip.Value);
            }

            if (take.HasValue && take.Value > 0)
            {
                result = result.Take(take.Value);
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, int? skip = null, int? take = null)
        {
            if (specification == null)
            {
                return await this.FindAllAsync(skip, take).ConfigureAwait(false);
            }

            var result = this.entities.Where(specification.ToPredicate());

            if (skip.HasValue && skip.Value > 0)
            {
                result = result.Skip(skip.Value);
            }

            if (take.HasValue && take.Value > 0)
            {
                result = result.Take(take.Value);
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, int? skip = null, int? take = null)
        {
            var specsArray = specifications as ISpecification<T>[] ?? specifications.ToArray();
            var result = this.entities;

            foreach (var specification in specsArray.NullToEmpty())
            {
                result = result.Where(specification.ToPredicate());
            }

            if(skip.HasValue && skip.Value > 0)
            {
                result = result.Skip(skip.Value);
            }

            if (take.HasValue && take.Value > 0)
            {
                result = result.Take(take.Value);
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">id</exception>
        public async Task<T> FindOneAsync(object id)
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

            return await this.FindOneAsync(id) != null;
        }

        /// <summary>
        /// Adds or updates asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Method for generating new Ids not provided</exception>
        public async Task<T> AddOrUpdateAsync(T entity)
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
                await this.mediator.Publish(new EntityAddedDomainEvent<T>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<T>(entity)).ConfigureAwait(false);
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
                await this.mediator.Publish(new EntityDeletedDomainEvent<T>(entity)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Id</exception>
        public async Task DeleteAsync(T entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return;
            }

            this.entities = this.entities.Where(x => !x.Id.Equals(entity.Id));
            await this.mediator.Publish(new EntityDeletedDomainEvent<T>(entity)).ConfigureAwait(false);
        }
    }
}
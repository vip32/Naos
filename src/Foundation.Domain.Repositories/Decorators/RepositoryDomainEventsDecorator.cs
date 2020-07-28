namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;

#pragma warning disable SA1629 // Documentation text should end with a period
    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IGenericRepository{TEntity}"/>.</para>
    /// <para>
    ///
    ///                          |Aggregate |
    ///                          `----------`            | RepositoryDomain|      |(repository)|
    ///      | Client |               | +DomainEvents    | EventsDecorator |      | Decoratee  |
    ///      `--------`               |                  `-----------------`      `------------`
    ///          x------------------->|                        |                      |
    ///          |   Register(event)  |                        |                      |
    ///          |                    |                        |                      |
    ///          |                    |                        |                      |
    ///          x--------------------|----------------------->|                      |
    ///          |                    |   Insert(aggregate)    x--------------------->|
    ///          |                    |                        x        Insert()      |
    ///          |                    |                        x                      |
    ///          |                    |                        x<---------------------x
    ///          |                    |                        x                      |
    ///          |                    |                        x--.                   |
    ///          |                    |<-----------------------x  |Publish            |
    ///          |                    |      GetAll()::events  x  |Registered         |
    ///          |                    |<-----------------------x  |DomainEvents       |
    ///          |                    |      Clear()           |<-`                   |
    ///
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositoryDomainEventsDecorator<TEntity> : IGenericRepository<TEntity>
#pragma warning restore SA1629 // Documentation text should end with a period
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IGenericRepository<TEntity>> logger;
        private readonly IMediator mediator;
        private readonly IGenericRepository<TEntity> inner;

        public RepositoryDomainEventsDecorator(
            ILogger<IGenericRepository<TEntity>> logger,
            IMediator mediator,
            IGenericRepository<TEntity> inner)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.logger = logger;
            this.mediator = mediator;
            this.inner = inner;
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.inner.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.inner.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.inner.FindOneAsync(id).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            // publish all domain events before transaction ends
            foreach (var @event in entity?.DomainEvents.GetAll()) // or use entity?.DomainEvents.DispatchAsync
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();
            return await this.inner.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            // publish all domain events before transaction ends
            foreach (var @event in entity?.DomainEvents.GetAll()) // or use entity?.DomainEvents.DispatchAsync
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();
            return await this.inner.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            // publish all domain events before transaction ends
            foreach (var @event in entity?.DomainEvents.GetAll()) // or use entity?.DomainEvents.DispatchAsync
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();
            return await this.inner.UpsertAsync(entity).AnyContext();
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            return await this.inner.DeleteAsync(id).AnyContext();
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            // publish all domain events before transaction ends
            foreach (var @event in entity?.DomainEvents.GetAll()) // or use entity?.DomainEvents.DispatchAsync
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();

            return await this.inner.DeleteAsync(entity).AnyContext();
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
    }
}

namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    .-----------.        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    .------------.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IRepository{TEntity}" />
    public class RepositoryDomainEventsDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IRepository<TEntity>> logger;
        private readonly IMediator mediator;
        private readonly IRepository<TEntity> decoratee;

        public RepositoryDomainEventsDecorator(ILogger<IRepository<TEntity>> logger, IMediator mediator, IRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.logger = logger;
            this.mediator = mediator;
            this.decoratee = decoratee;
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            return await this.decoratee.DeleteAsync(id).AnyContext();
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            foreach(var @event in entity?.DomainEvents.GetAll())
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();

            return await this.decoratee.DeleteAsync(entity).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(specification, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            foreach(var @event in entity?.DomainEvents.GetAll())
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();
            return await this.decoratee.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            foreach(var @event in entity?.DomainEvents.GetAll())
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();
            return await this.decoratee.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            foreach(var @event in entity?.DomainEvents.GetAll())
            {
                this.logger.LogInformation($"{{LogKey:l}} publish (type={@event.GetType().Name.Replace("DomainEvent", string.Empty)})", LogKeys.DomainEvent);
                await this.mediator.Publish(@event).AnyContext();
            }

            entity?.DomainEvents.Clear();
            return await this.decoratee.UpsertAsync(entity).AnyContext();
        }
    }
}

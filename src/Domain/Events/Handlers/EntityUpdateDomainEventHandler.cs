namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public abstract class EntityUpdateDomainEventHandler
        : IDomainEventHandler<EntityUpdateDomainEvent>
    {
        private readonly ILogger<EntityUpdateDomainEventHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdateDomainEventHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected EntityUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        /// Determines whether this instance can handle the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <returns>
        /// <c>true</c> if this instance can handle the specified notification; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CanHandle(EntityUpdateDomainEvent notification);

        /// <summary>
        /// Handles the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task Handle(EntityUpdateDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if(this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} [{notification.Id}] handle {notification.GetType().Name.SliceTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogKeys.DomainEvent);

                    if(notification?.Entity.Is<IStateEntity>() == true)
                    {
                        var entity = notification.Entity.As<IStateEntity>();
                        entity.State?.SetUpdated("[IDENTITY]", "domainevent"); // TODO: use current identity
                    }

                    if(notification?.Entity.Is<IIdentifiable>() == true)
                    {
                        var entity = notification.Entity.As<IIdentifiable>();
                        entity.SetIdentifierHash();
                    }
                }
            });
        }
    }
}

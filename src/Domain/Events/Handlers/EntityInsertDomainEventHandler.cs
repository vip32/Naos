namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public abstract class EntityInsertDomainEventHandler
        : IDomainEventHandler<EntityInsertDomainEvent>
    {
        private readonly ILogger<EntityInsertDomainEventHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInsertDomainEventHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected EntityInsertDomainEventHandler(ILogger<EntityInsertDomainEventHandler> logger)
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
        public abstract bool CanHandle(EntityInsertDomainEvent notification);

        /// <summary>
        /// Handles the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task Handle(EntityInsertDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if(this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} [{notification.Id}] handle {notification.GetType().Name.SliceTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogKeys.DomainEvent);

                    if(notification?.Entity.Is<IStateEntity>() == true) // use pattern matching
                    {
                        var entity = notification.Entity.As<IStateEntity>();
                        entity.State?.SetCreated("[IDENTITY]", "domainevent"); // TODO: use current identity
                    }

                    if(notification?.Entity.Is<IIdentifiable>() == true) // use pattern matching
                    {
                        var entity = notification.Entity.As<IIdentifiable>();
                        entity.SetIdentifierHash();
                    }
                }
            });
        }
    }
}

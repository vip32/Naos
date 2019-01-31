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

        public EntityUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public abstract bool CanHandle(EntityUpdateDomainEvent notification);

        public virtual async Task Handle(EntityUpdateDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} [{notification.Identifier}] handle {notification.GetType().Name.SubstringTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogEventKeys.DomainEvent);

                    if (notification?.Entity.Is<IStateEntity>() == true)
                    {
                        var entity = notification.Entity.As<IStateEntity>();
                        entity.State?.SetUpdated("[IDENTITY]", "domainevent"); // TODO: use current identity
                    }

                    if (notification?.Entity.Is<IIdentifiable>() == true)
                    {
                        var entity = notification.Entity.As<IIdentifiable>();
                        entity.SetIdentifierHash();
                    }
                }
            });
        }
    }
}

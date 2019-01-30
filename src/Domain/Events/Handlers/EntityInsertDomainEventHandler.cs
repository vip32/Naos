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

        public EntityInsertDomainEventHandler(ILogger<EntityInsertDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public abstract bool CanHandle(EntityInsertDomainEvent notification);

        public virtual async Task Handle(EntityInsertDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} handle {notification.GetType().Name.SubstringTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogEventKeys.DomainEvent);

                    if (notification?.Entity.Is<IStateEntity>() == true)
                    {
                        var entity = notification.Entity.As<IStateEntity>();
                        entity.State?.SetCreated("[IDENTITY]", "domainevent"); // TODO: use current identity
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

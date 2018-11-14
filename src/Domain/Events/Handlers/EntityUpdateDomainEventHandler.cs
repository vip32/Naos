namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class EntityUpdateDomainEventHandler
        : IDomainEventHandler<EntityUpdateDomainEvent<IEntity>>
    {
        private readonly ILogger<EntityUpdateDomainEventHandler> logger;

        public EntityUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public async Task Handle(EntityUpdateDomainEvent<IEntity> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                this.logger.LogInformation($"update entity: {notification.Entity.GetType().PrettyName()} handled by {this.GetType().PrettyName()}");

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
            });
        }
    }
}

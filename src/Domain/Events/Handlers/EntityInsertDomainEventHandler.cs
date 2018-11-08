namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class EntityInsertDomainEventHandler
        : IDomainEventHandler<EntityInsertDomainEvent<IEntity>>
    {
        private readonly ILogger<EntityInsertDomainEventHandler> logger;

        public EntityInsertDomainEventHandler(ILogger<EntityInsertDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public async Task Handle(EntityInsertDomainEvent<IEntity> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                this.logger.LogInformation($"insert entity: {notification.Entity.GetType().PrettyName()} handled by {this.GetType().PrettyName()}");

                if (notification?.Entity.Is<IStateEntity>() == true)
                {
                    var entity = notification.Entity.As<IStateEntity>();
                    entity.State?.SetCreated("[IDENTITY]", "domainevent"); // TODO
                    entity.State?.UpdateIdentifierHash(notification.Entity);

                    this.logger.LogInformation($"insert entity: {entity.GetType().PrettyName()}, identifier: {entity.State.IdentifierHash}");
                }
            });
        }
    }
}

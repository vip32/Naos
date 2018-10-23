namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;

    public class EntityUpdateDomainEventDomainEventHandler
        : IDomainEventHandler<EntityUpdateDomainEvent<IEntity>>
    {
        public async Task Handle(EntityUpdateDomainEvent<IEntity> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (notification?.Entity.Is<IStateEntity>() == true)
                {
                    notification.Entity.As<IStateEntity>().State.SetUpdated("[IDENTITY]", "domainevent");
                }

                if (notification?.Entity.Is<IVersionIdentified>() == true)
                {
                    notification.Entity.As<IVersionIdentified>().UpdateVersionIdentifier();
                }
            });
        }
    }
}

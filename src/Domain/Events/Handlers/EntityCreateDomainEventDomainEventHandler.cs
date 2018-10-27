namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;

    public class EntityCreateDomainEventDomainEventHandler
        : IDomainEventHandler<EntityCreateDomainEvent<IEntity>>
    {
        public async Task Handle(EntityCreateDomainEvent<IEntity> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (notification?.Entity.Is<IStateEntity>() == true)
                {
                    notification.Entity.As<IStateEntity>().State.SetCreated("[IDENTITY]", "domainevent");
                }

                if (notification?.Entity.Is<IVersionedEntity>() == true)
                {
                    notification.Entity.As<IVersionedEntity>().UpdateVersionIdentifier();
                }
            });
        }
    }
}

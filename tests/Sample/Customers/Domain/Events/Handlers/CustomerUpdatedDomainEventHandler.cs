namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CustomerUpdatedDomainEventHandler
        : IDomainEventHandler<EntityUpdatedDomainEvent<IEntity>>
    {
        public async Task Handle(EntityUpdatedDomainEvent<IEntity> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (notification?.Entity.Is<Customer>() == true)
                {
                    var entity = notification.Entity.As<Customer>();
                    // TODO: do something, trigger message (integration)
                }
            });
        }
    }
}
namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CustomerInsertedDomainEventHandler
        : IDomainEventHandler<EntityInsertedDomainEvent<IEntity>>
    {
        public async Task Handle(EntityInsertedDomainEvent<IEntity> notification, CancellationToken cancellationToken)
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

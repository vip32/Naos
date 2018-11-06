namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Domain;

    public class CustomerUpdatedDomainEventHandler
        : IDomainEventHandler<EntityUpdatedDomainEvent<Customer>>
    {
        public async Task Handle(EntityUpdatedDomainEvent<Customer> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                // TODO: do something, trigger message (integration)
            });
        }
    }
}
namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Domain;

    public class CustomerInsertedDomainEventHandler
        : IDomainEventHandler<EntityInsertedDomainEvent<Customer>>
    {
        public async Task Handle(EntityInsertedDomainEvent<Customer> notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                // TODO: do something, trigger message (integration)
            });
        }
    }
}

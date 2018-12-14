namespace Naos.Sample.Customers.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CustomerUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        public CustomerUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<Customer>() == true;
        }
    }
}
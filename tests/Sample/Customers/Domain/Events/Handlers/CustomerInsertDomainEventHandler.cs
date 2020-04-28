namespace Naos.Sample.Customers.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CustomerInsertDomainEventHandler
        : EntityInsertDomainEventHandler
    {
        public CustomerInsertDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityInsertDomainEvent notification)
        {
            return notification?.Entity.Is<Customer>() == true;
        }
    }
}

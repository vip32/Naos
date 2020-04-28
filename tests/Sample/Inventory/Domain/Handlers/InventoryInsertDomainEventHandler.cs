namespace Naos.Sample.Inventory.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class InventoryInsertDomainEventHandler
        : EntityInsertDomainEventHandler
    {
        public InventoryInsertDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityInsertDomainEvent notification)
        {
            return notification?.Entity.Is<ProductInventory>() == true;
        }
    }
}

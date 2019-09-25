namespace Naos.Sample.Inventory.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class InventoryUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        public InventoryUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<ProductInventory>() == true;
        }
    }
}
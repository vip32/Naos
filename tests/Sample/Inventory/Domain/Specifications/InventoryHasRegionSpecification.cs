namespace Naos.Sample.Inventory.Domain
{
    using Naos.Foundation.Domain;

    public class InventoryHasRegionSpecification : Specification<ProductInventory>
    {
        public InventoryHasRegionSpecification(string region)
            : base(e => e.Region == region)
        {
        }
    }
}

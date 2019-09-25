namespace Naos.Sample.Inventory.Domain
{
    using Naos.Foundation.Domain;

    public class HasRegionSpecification : Specification<ProductInventory>
    {
        public HasRegionSpecification(string region)
            : base(e => e.Region == region)
        {
        }
    }
}

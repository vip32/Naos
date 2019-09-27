namespace Naos.Sample.Inventory.Domain
{
    using Naos.Foundation.Domain;

    public class ReplenishmentHasRegionSpecification : Specification<ProductReplenishment>
    {
        public ReplenishmentHasRegionSpecification(string region)
            : base(e => e.Region == region)
        {
        }
    }
}

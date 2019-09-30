namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasEastRegionOrderSpecification : Specification<Order>
    {
        public HasEastRegionOrderSpecification()
            : base(e => e.Region == "East")
        {
        }
    }
}

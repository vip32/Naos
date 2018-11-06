namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasEastRegionSpecification : Specification<Customer>
    {
        public HasEastRegionSpecification()
            : base(e => e.Region == "East")
        {
        }
    }
}

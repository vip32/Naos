namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasEastRegionSpecification : Specification<Customer>
    {
        public HasEastRegionSpecification()
            : base(e => e.Region == "East")
        {
        }
    }
}

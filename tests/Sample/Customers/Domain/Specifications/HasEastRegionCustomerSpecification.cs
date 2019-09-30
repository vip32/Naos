namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasEastRegionCustomerSpecification : Specification<Customer>
    {
        public HasEastRegionCustomerSpecification()
            : base(e => e.Region == "East")
        {
        }
    }
}

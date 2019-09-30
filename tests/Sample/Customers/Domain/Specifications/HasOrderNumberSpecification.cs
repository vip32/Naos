namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasOrderNumberSpecification : Specification<Order>
    {
        public HasOrderNumberSpecification(string value)
            : base(e => e.OrderNumber == value)
        {
        }
    }
}

namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasCustomerNumberSpecification : Specification<Customer>
    {
        public HasCustomerNumberSpecification(string value)
            : base(e => e.CustomerNumber == value)
        {
        }
    }
}

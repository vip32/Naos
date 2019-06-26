namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasNumberSpecification : Specification<Customer>
    {
        public HasNumberSpecification(string value)
            : base(e => e.CustomerNumber == value)
        {
        }
    }
}

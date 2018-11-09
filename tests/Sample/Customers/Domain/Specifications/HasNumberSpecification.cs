namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasNumberSpecification : Specification<Customer>
    {
        public HasNumberSpecification(string value)
            : base(e => e.CustomerNumber == value)
        {
        }
    }
}

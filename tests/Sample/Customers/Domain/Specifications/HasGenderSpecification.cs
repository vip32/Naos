namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasGenderSpecification : Specification<Customer>
    {
        public HasGenderSpecification(string value = "Male")
            : base(e => e.Gender == value)
        {
        }
    }
}

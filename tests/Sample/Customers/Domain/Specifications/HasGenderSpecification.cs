namespace Naos.Sample.Customers.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasGenderSpecification : Specification<Customer>
    {
        public HasGenderSpecification(string gender = "Male")
            : base(e => e.Gender == gender)
        {
        }
    }
}

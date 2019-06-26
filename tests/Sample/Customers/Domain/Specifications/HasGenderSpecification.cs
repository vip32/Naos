namespace Naos.Sample.Customers.Domain
{
    using Naos.Foundation.Domain;

    public class HasGenderSpecification : Specification<Customer>
    {
        public HasGenderSpecification(string value = "Male")
            : base(e => e.Gender == value)
        {
        }
    }
}

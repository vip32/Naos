namespace Naos.Sample.Countries.Domain
{
    using Naos.Foundation.Domain;

    public class HasCodeSpecification : Specification<Country>
    {
        public HasCodeSpecification(string value)
            : base(e => e.Code == value)
        {
        }
    }
}

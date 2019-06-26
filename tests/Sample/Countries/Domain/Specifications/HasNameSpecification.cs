namespace Naos.Sample.Countries.Domain
{
    using Naos.Foundation.Domain;

    public class HasNameSpecification : Specification<Country>
    {
        public HasNameSpecification(string value)
            : base(e => e.Name == value)
        {
        }
    }
}

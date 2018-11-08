namespace Naos.Sample.Countries.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasNameSpecification : Specification<Country>
    {
        public HasNameSpecification(string value)
            : base(e => e.Name == value)
        {
        }
    }
}

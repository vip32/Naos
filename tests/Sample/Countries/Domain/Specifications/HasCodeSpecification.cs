namespace Naos.Sample.Countries.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasCodeSpecification : Specification<Country>
    {
        public HasCodeSpecification(string value)
            : base(e => e.Code == value)
        {
        }
    }
}

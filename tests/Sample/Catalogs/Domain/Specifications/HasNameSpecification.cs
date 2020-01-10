namespace Naos.Sample.Catalogs.Domain
{
    using Naos.Foundation.Domain;

    public class HasNameSpecification : Specification<Product>
    {
        public HasNameSpecification(string value)
            : base(e => e.Name == value)
        {
        }
    }
}

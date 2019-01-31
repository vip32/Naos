namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasEastDomainSpecification : Specification<UserAccount>
    {
        public HasEastDomainSpecification()
            : base(e => e.Domain == "East")
        {
        }
    }
}

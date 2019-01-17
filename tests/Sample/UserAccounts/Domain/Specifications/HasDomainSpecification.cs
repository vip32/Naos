namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasDomainSpecification : Specification<UserAccount>
    {
        public HasDomainSpecification(string domain)
            : base(e => e.Domain == domain)
        {
        }
    }
}

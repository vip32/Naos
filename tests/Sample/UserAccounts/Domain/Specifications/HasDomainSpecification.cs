namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Foundation.Domain;

    public class HasDomainSpecification : Specification<UserAccount>
    {
        public HasDomainSpecification(string domain)
            : base(e => e.AdAccount.Domain == domain)
        {
        }
    }
}

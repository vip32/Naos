namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Foundation.Domain;

    public class HasEastDomainSpecification : Specification<UserAccount>
    {
        public HasEastDomainSpecification()
            : base(e => e.AdAccount.Domain == "East")
        {
        }
    }
}

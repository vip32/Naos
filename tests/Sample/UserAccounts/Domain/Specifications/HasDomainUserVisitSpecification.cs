namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Foundation.Domain;

    public class HasDomainUserVisitSpecification : Specification<UserVisit>
    {
        public HasDomainUserVisitSpecification(string domain)
            : base(e => e.Region == domain)
        {
        }
    }
}

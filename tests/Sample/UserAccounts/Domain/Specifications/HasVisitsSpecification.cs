namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Foundation.Domain;

    public class HasVisitsSpecification : Specification<UserAccount>
    {
        public HasVisitsSpecification()
            : base(e => e.VisitCount > 0)
        {
        }
    }
}

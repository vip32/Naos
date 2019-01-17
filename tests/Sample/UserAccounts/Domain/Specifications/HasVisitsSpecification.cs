namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Core.Domain.Specifications;

    public class HasVisitsSpecification : Specification<UserAccount>
    {
        public HasVisitsSpecification()
            : base(e => e.VisitCount > 0)
        {
        }
    }
}

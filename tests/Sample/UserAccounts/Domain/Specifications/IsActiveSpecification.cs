namespace Naos.Sample.UserAccounts.Domain
{
    using System;
    using System.Linq.Expressions;
    using LinqKit;
    using Naos.Foundation.Domain;

    public class IsActiveSpecification : Specification<UserAccount>
    {
        private readonly int recentVisitDays;
        private readonly int visits;

        public IsActiveSpecification(int recentVisitDays = 2, int visits = 1)
        {
            this.recentVisitDays = recentVisitDays;
            this.visits = visits;
        }

        public override Expression<Func<UserAccount, bool>> ToExpression()
        {
            return this
                .HasRecentVisit(this.recentVisitDays)
                .And(p => p.VisitCount >= this.visits); // And = linqkit
        }

        private Expression<Func<UserAccount, bool>> HasRecentVisit(int recentVisitDays = 2)
        {
            return p => (p.RegisterDate != null && p.RegisterDate <= DateTime.UtcNow) &&
                        (p.LastVisitDate != null && p.LastVisitDate >= DateTime.UtcNow.AddDays(-recentVisitDays)); // less than x days old
        }
    }
}

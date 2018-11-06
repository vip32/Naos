namespace Naos.Sample.UserAccounts.Domain
{
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class UserAccount : Entity<string>, IAggregateRoot
    {
        public string Email { get; set; }

        public int VisitCount { get; set; }

        public DateTimeEpoch LastVisitDate { get; set; }

        public DateTimeEpoch RegisterDate { get; set; }
    }
}

namespace Naos.Sample.UserAccounts.Domain
{
    using System;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class UserAccount : AggregateRoot<Guid>, ITenantEntity
    {
        public string Email { get; set; }

        public int VisitCount { get; set; }

        public DateTimeEpoch LastVisitDate { get; set; }

        public DateTimeEpoch RegisterDate { get; set; }

        public string TenantId { get; set; }

        public AdAccount AdAccount { get; set; }
    }
}

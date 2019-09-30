namespace Naos.Sample.UserAccounts.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class UserVisit : AggregateRoot<Guid>, ITenantEntity
    {
        public string Email { get; set; }

        public string Region { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public string TenantId { get; set; }
    }
}

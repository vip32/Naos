namespace Naos.Sample.UserAccounts.Infrastructure
{
    using System;

    public class DtoUserVisit
    {
        public Guid Id { get; set; }

        public string EmailAddress { get; set; }

        public string Location { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public string OwnerId { get; set; }
    }
}

namespace Naos.Sample.Inventory.Infrastructure
{
    using System;
    using Naos.Foundation.Domain;

    public class DtoProductReplenishment
    {
        public string Id { get; set; }

        public string ProductNumber { get; set; }

        public int Amount { get; set; }

        public string Region { get; set; }

        public string OwnerId { get; set; }

        public string FromLocation { get; set; }

        public DateTimeOffset ShipDate { get; set; } = DateTimeOffset.UtcNow;

        public string AtLocation { get; set; }

        public DateTimeOffset ArriveDate { get; set; } = DateTimeOffset.UtcNow;

        public string IdentifierHash { get; set; }

        public State State { get; set; } = new State();
    }
}

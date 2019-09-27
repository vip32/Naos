namespace Naos.Sample.Inventory.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class ProductReplenishment : AggregateRoot<string>, ITenantEntity
    {
        public string Number { get; set; }

        public int Quantity { get; set; }

        public string Region { get; set; }

        public string TenantId { get; set; }

        public string ShippedFromLocation { get; set; }

        public DateTimeOffset ShippedDate { get; set; } = DateTimeOffset.UtcNow;

        public string ArrivedAtLocation { get; set; }

        public DateTimeOffset ArrivedDate { get; set; } = DateTimeOffset.UtcNow;
    }
}

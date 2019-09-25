namespace Naos.Sample.Inventory.Domain
{
    using Naos.Foundation.Domain;

    public class ProductInventory : AggregateRoot<string>, ITenantEntity
    {
        public string Number { get; set; }

        public int Quantity { get; set; }

        public string Region { get; set; }

        public string TenantId { get; set; }

        public bool HasStock => this.Quantity > 0;
    }
}

namespace Naos.Sample.Inventory.Domain
{
    using Naos.Foundation.Domain;

    public class OutOfStockSpecification : Specification<ProductInventory>
    {
        public OutOfStockSpecification()
            : base(e => e.Quantity == 0)
        {
        }
    }
}

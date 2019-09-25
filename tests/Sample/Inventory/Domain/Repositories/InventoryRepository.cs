namespace Naos.Sample.Inventory.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class InventoryRepository : GenericRepository<ProductInventory>, IInventoryRepository
    {
        public InventoryRepository(IGenericRepository<ProductInventory> decoratee)
            : base(decoratee)
        {
        }

        public async Task<IEnumerable<ProductInventory>> FindOneOutOfStock()
            => await this.FindAllAsync(new OutOfStockSpecification()).AnyContext();
    }
}

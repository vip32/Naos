namespace Naos.Sample.Inventory.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    public interface IInventoryRepository : IGenericRepository<ProductInventory>
    {
        Task<IEnumerable<ProductInventory>> FindOneOutOfStock();
    }
}

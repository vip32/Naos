namespace Naos.Sample.Inventory.Domain
{
    using Naos.Foundation.Domain;

    public class ReplenishmentRepository : GenericRepository<ProductReplenishment>, IReplenishmentRepository
    {
        public ReplenishmentRepository(IGenericRepository<ProductReplenishment> decoratee)
            : base(decoratee)
        {
        }
    }
}

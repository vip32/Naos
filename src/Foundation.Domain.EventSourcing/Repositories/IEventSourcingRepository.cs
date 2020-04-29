namespace Naos.Foundation.Domain
{
    using System.Threading.Tasks;

    public interface IEventSourcingRepository<TAggregate, TId>
        where TAggregate : IEventSourcedAggregateRoot<TId>
    {
        Task<TAggregate> GetByIdAsync(TId id);

        Task SaveAsync(TAggregate aggregate);
    }
}

namespace Naos.Foundation.Domain
{
    using System.Threading.Tasks;

    public interface IEventSourcingRepository<TAggregate, TId>
        where TAggregate : IEventSourcedAggregateRoot<TId>
    {
        Task<TAggregate> GetByIdAsync(TId aggregateId);

        Task SaveAsync(TAggregate aggregate);

        Task<TAggregate> GetSnapshotByIdAsync(TId aggregateId);

        Task SaveSnapshotAsync(TAggregate aggregate);
    }
}

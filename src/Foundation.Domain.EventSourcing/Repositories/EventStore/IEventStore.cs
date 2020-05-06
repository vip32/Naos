namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(string streamName, long? fromVersion = null, long? toVersion = null);

        Task<EventResult> SaveEventAsync<TId>(string streamName, IDomainEvent<TId> @event);

        public Task<Snapshot<TAggregate, TId>> ReadSnapshotAsync<TAggregate, TId>(string streamName)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>;

        public Task<EventResult> SaveSnapshotAsync<TAggregate, TId>(string streamName, TAggregate aggregate)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>;
    }
}

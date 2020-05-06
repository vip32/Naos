namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class InMemoryEventStore : IEventStore
    {
        private readonly List<object> events = new List<object>();

        public Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(string streamName, long? fromVersion = null, long? toVersion = null)
        {
            fromVersion ??= -1;
            toVersion ??= long.MaxValue;

            return Task.FromResult(this.events
                    .Select(e => e as Event<TId>)
                    .Where(e => e != null
                            && e.DomainEvent.AggregateVersion > fromVersion
                            && e.DomainEvent.AggregateVersion < toVersion
                            && e.DomainEvent.AggregateId.ToString() == streamName.SliceFrom("-"))
                    .OrderBy(e => e.DomainEvent.AggregateVersion).AsEnumerable());
        }

        public Task<EventResult> SaveEventAsync<TId>(string streamName, IDomainEvent<TId> @event)
        {
            this.events.Add(new Event<TId>(@event, 0));

            return Task.Run(() => new EventResult(@event.AggregateVersion + 1));
        }

        public Task<Snapshot<TAggregate, TId>> ReadSnapshotAsync<TAggregate, TId>(string streamName)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        {
            throw new System.NotImplementedException();
        }

        public Task<EventResult> SaveSnapshotAsync<TAggregate, TId>(string streamName, TAggregate aggregate)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        {
            throw new System.NotImplementedException();
        }
    }
}

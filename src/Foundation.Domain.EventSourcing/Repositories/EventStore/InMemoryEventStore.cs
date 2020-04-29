namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class InMemoryEventStore : IEventStore
    {
        private readonly List<object> events = new List<object>();

        public Task<AppendResult> AppendEventAsync<TId>(IDomainEvent<TId> @event)
        {
            this.events.Add(new Event<TId>(@event, 0));

            return Task.Run(() => new AppendResult(@event.AggregateVersion + 1));
        }

        public Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(TId id, long? fromVersion = null, long? toVersion = null)
        {
            fromVersion ??= -1;
            toVersion ??= long.MaxValue;

            return Task.FromResult(this.events
                    .Select(e => e as Event<TId>)
                    .Where(e => e != null
                            && e.DomainEvent.AggregateVersion > fromVersion
                            && e.DomainEvent.AggregateVersion < toVersion
                            && e.DomainEvent.AggregateId.ToString() == id.ToString())
                    .OrderBy(e => e.DomainEvent.AggregateVersion).AsEnumerable());
        }
    }
}

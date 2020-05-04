namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<AppendResult> AppendEventAsync<TId>(string streamName, IDomainEvent<TId> @event);

        Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(string streamName, long? fromVersion = null, long? toVersion = null);

        // TODO: snapshot methods
    }
}

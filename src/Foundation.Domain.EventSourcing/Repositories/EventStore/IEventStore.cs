namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<AppendResult> AppendEventAsync<TId>(string streamName, IDomainEvent<TId> @event);
            //where TAggregateId : IAggregateId;

        Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(string streamName, long? fromVersion = null, long? toVersion = null);
            //where TAggregateId : IAggregateId;
    }
}

namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<AppendResult> AppendEventAsync<TId>(IDomainEvent<TId> @event);
            //where TAggregateId : IAggregateId;

        Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(TId id, long? fromVersion = null, long? toVersion = null);
            //where TAggregateId : IAggregateId;
    }
}

namespace Naos.Foundation.Domain.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(TId id);
            //where TAggregateId : IAggregateId;

        Task<AppendResult> AppendEventAsync<TAggregateId>(IDomainEvent<TAggregateId> @event);
            //where TAggregateId : IAggregateId;
    }
}

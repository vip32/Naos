namespace Naos.Foundation.Infrastructure.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Domain.EventSourcing;

    public class SqlServerEventStore : IEventStore // inspiration: https://github.com/bolicd/eventstore/blob/master/Infrastructure/Repositories/EventStoreRepository.cs
    {
        public Task<AppendResult> AppendEventAsync<TId>(string streamName, IDomainEvent<TId> @event)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(string streamName, long? fromVersion = null, long? toVersion = null)
        {
            throw new System.NotImplementedException();
        }
    }
}

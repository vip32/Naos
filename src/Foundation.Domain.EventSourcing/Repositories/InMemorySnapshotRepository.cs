namespace Naos.Foundation.Domain
{
    using System;
    using System.Threading.Tasks;

    public class InMemorySnapshotRepository<TAggregate, TId> : ISnapshotRepository<TAggregate, TId>
        where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        //where TId : IAggregateId
    {
        public Task<TAggregate> GetByIdAsync(TId aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(TAggregate aggregate)
        {
            throw new NotImplementedException();
        }

        private string GetStreamName(TId aggregateId)
        {
            return $"{typeof(TAggregate).Name}-{aggregateId}-snapshot";
        }
    }
}

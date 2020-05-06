namespace Naos.Foundation.Domain.EventSourcing
{
    public class Snapshot<TAggregate, TId> // wrapper
        where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
    {
        public Snapshot(TAggregate aggregate, long number)
        {
            this.Aggregate = aggregate;
            this.EventNumber = number;
        }

        public long EventNumber { get; }

        public TAggregate Aggregate { get; }
    }
}

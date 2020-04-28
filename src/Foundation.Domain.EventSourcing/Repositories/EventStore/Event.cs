namespace Naos.Foundation.Domain.EventSourcing
{
    public class Event<TId>
    {
        public Event(IDomainEvent<TId> domainEvent, long eventNumber)
        {
            this.DomainEvent = domainEvent;
            this.EventNumber = eventNumber;
        }

        public long EventNumber { get; }

        public IDomainEvent<TId> DomainEvent { get; }
    }
}

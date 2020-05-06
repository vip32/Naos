namespace Naos.Foundation.Domain.EventSourcing
{
    public class Event<TId> // wrapper
    {
        public Event(IDomainEvent<TId> @event, long number)
        {
            this.DomainEvent = @event;
            this.EventNumber = number;
        }

        public long EventNumber { get; }

        public IDomainEvent<TId> DomainEvent { get; }
    }
}

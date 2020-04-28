namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Linq;

#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable SA1402 // File may only contain a single type
    public abstract class EventSourcingAggregateRoot<TId> : Entity<TId>, IEventSourcingAggregate<TId>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public const long NewAggregateVersion = -1;

        private readonly ICollection<IDomainEvent<TId>> uncommittedEvents = new LinkedList<IDomainEvent<TId>>();
        private long version = NewAggregateVersion;

        long IEventSourcingAggregate<TId>.Version => this.version;

        void IEventSourcingAggregate<TId>.ApplyEvent(IDomainEvent<TId> @event, long version)
        {
            if (!this.uncommittedEvents.Any(x => Equals(x.EventId, @event.EventId)))
            {
                ((dynamic)this).Apply((dynamic)@event);
                this.version = version;
            }
        }

        void IEventSourcingAggregate<TId>.ClearUncommittedEvents()
        {
            this.uncommittedEvents.Clear();
        }

        IEnumerable<IDomainEvent<TId>> IEventSourcingAggregate<TId>.GetUncommittedEvents()
        {
            return this.uncommittedEvents.AsEnumerable();
        }

        protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : DomainEventBase<TId>
        {
            var eventWithAggregate = @event.WithAggregate(
                Equals(this.Id, default(TId)) ? @event.AggregateId : this.Id,
                this.version);

            ((IEventSourcingAggregate<TId>)this).ApplyEvent(eventWithAggregate, this.version + 1);
            this.uncommittedEvents.Add(eventWithAggregate);
        }
    }
}

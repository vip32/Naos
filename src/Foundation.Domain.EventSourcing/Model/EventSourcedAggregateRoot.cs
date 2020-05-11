namespace Naos.Foundation.Domain
{
    using System.Collections.Generic;
    using System.Linq;

#pragma warning disable CA1033 // Interface methods should be callable by child types
    public abstract class EventSourcedAggregateRoot<TId> : Entity<TId>, IEventSourcedAggregateRoot<TId>
    {
        public const long NewVersion = -1;

        private readonly ICollection<IDomainEvent<TId>> changes = new LinkedList<IDomainEvent<TId>>();
        private long version = NewVersion;

        long IEventSourcedAggregateRoot<TId>.Version => this.version;

        void IEventSourcedAggregateRoot<TId>.ApplyEvent(IDomainEvent<TId> @event, long version)
        {
            if (!this.changes.Any(x => Equals(x.EventId, @event.EventId)))
            {
                ((dynamic)this).Apply((dynamic)@event);
                this.version = version;
            }
        }

        void IEventSourcedAggregateRoot<TId>.ClearChanges()
            => this.changes.Clear();

        IEnumerable<IDomainEvent<TId>> IEventSourcedAggregateRoot<TId>.GetChanges()
            => this.changes.AsEnumerable();

        protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : DomainEventBase<TId>
        {
            var eventWithAggregate = @event.ForAggregate(
                Equals(this.Id, default(TId)) ? @event.AggregateId : this.Id,
                this.version);

            ((IEventSourcedAggregateRoot<TId>)this).ApplyEvent(eventWithAggregate, this.version + 1);
            this.changes.Add(eventWithAggregate);
        }
    }
}

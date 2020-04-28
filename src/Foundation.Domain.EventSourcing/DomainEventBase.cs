namespace Naos.Foundation.Domain
{
    using System;

#pragma warning disable SA1402 // File may only contain a single type
    public abstract class DomainEventBase<TId> : DomainEventBase, IDomainEvent<TId>
#pragma warning restore SA1402 // File may only contain a single type
    {
        protected DomainEventBase()
        {
            this.EventId = Guid.NewGuid();
        }

        protected DomainEventBase(TId aggregateId)
            : this()
        {
            this.AggregateId = aggregateId;
        }

        protected DomainEventBase(TId aggregateId, long aggregateVersion)
            : this(aggregateId)
        {
            this.AggregateVersion = aggregateVersion;
        }

        public TId AggregateId { get; private set; }

        public long AggregateVersion { get; private set; }

        public abstract IDomainEvent<TId> WithAggregate(TId aggregateId, long aggregateVersion);
    }
}

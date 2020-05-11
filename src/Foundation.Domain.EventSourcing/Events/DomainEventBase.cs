namespace Naos.Foundation.Domain
{
    using System;

    public abstract class DomainEventBase<TId> : DomainEventBase, IDomainEvent<TId>
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

        //public abstract IDomainEvent<TId> WithAggregate(TId aggregateId, long aggregateVersion);
        public IDomainEvent<TId> ForAggregate(TId aggregateId, long aggregateVersion)
        {
            this.AggregateId = aggregateId;
            this.AggregateVersion = aggregateVersion;
            return this;
        }
    }
}

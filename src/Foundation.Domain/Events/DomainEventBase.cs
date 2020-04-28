namespace Naos.Foundation.Domain
{
    using System;

    public abstract class DomainEventBase : IDomainEvent, IEquatable<DomainEventBase>
    {
        private int? hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventBase"/> class.
        /// </summary>
        public DomainEventBase()
        {
            this.EventId = IdGenerator.Instance.Next;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventBase"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public DomainEventBase(string id, string correlationId)
        {
            this.EventId = id ?? IdGenerator.Instance.Next;
            this.CorrelationId = correlationId;
        }

        /// <summary>
        /// Gets or sets the identifier of this domain event.
        /// </summary>
        /// <value>
        /// The domain event identifier.
        /// </value>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        public bool Equals(DomainEventBase other)
        {
            return other != null && this.EventId.Equals(other.EventId, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.hashCode ?? (this.hashCode = this.EventId.GetHashCode() ^ 31).Value;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public abstract class DomainEvent<TId> : DomainEventBase, IDomainEvent<TId>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public TId AggregateId { get; }

        public long AggregateVersion { get; }
    }
}

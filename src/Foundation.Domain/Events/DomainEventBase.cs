namespace Naos.Foundation.Domain
{
    using System;

    public abstract class DomainEventBase : IDomainEvent, IEquatable<DomainEventBase>
    {
        private int? hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventBase"/> class.
        /// </summary>
        protected DomainEventBase()
        {
            this.EventId = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventBase"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        protected DomainEventBase(Guid id, string correlationId)
        {
            this.EventId = id;
            this.CorrelationId = correlationId;
        }

        /// <summary>
        /// Gets or sets the identifier of this domain event.
        /// </summary>
        /// <value>
        /// The domain event identifier.
        /// </value>
        public Guid EventId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        public bool Equals(DomainEventBase other)
        {
            return other != null && this.EventId.Equals(other.EventId);
        }

        public override int GetHashCode()
        {
            return this.hashCode ?? (this.hashCode = this.EventId.GetHashCode() ^ 31).Value;
        }
    }
}

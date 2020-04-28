namespace Naos.Foundation.Domain
{
    public class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        public DomainEvent()
        {
            this.EventId = IdGenerator.Instance.Next;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public DomainEvent(string id, string correlationId)
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
    }
}

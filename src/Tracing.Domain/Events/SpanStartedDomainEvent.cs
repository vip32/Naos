namespace Naos.Tracing.Domain
{
    using Naos.Foundation.Domain;

    public class SpanStartedDomainEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpanStartedDomainEvent"/> class.
        /// </summary>
        /// <param name="span">The span.</param>
        public SpanStartedDomainEvent(ISpan span)
        {
            this.Span = span;
        }

        /// <summary>
        /// Gets or sets the span.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public ISpan Span { get; set; }
    }
}

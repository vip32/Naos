namespace Naos.Core.Tracing.Domain
{
    using Naos.Foundation.Domain;

    public class SpanEndedDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpanEndedDomainEvent"/> class.
        /// </summary>
        /// <param name="span">The span.</param>
        public SpanEndedDomainEvent(ISpan span)
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

namespace Naos.Core.Operations.Domain
{
    using System;

    public class Span : ISpan
    {
        public Span(string traceId, string spanId)
        {
            this.TraceId = traceId;
            this.SpanId = spanId;
        }

        /// <summary>
        /// Globally unique. Every span in a trace shares this ID.
        /// </summary>
        public string TraceId { get; } // correlationid

        /// <summary>
        /// Unique within a trace. Each span within a trace contains a different ID.
        /// </summary>
        public string SpanId { get; }

        public DateTimeOffset StartedTimestamp { get; set; }

        public DateTimeOffset FinishedTimestamp { get; set; }

        public TimeSpan Duration => this.FinishedTimestamp - this.StartedTimestamp;

        public bool Failed { get; set; }

        public ISpan SetTag(string key, string value)
        {
            return this;
        }
    }
}
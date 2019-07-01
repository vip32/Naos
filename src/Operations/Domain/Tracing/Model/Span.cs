namespace Naos.Core.Operations.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class Span : ISpan
    {
        private DataDictionary tags = new DataDictionary();

        public Span(string traceId, string spanId)
        {
            this.TraceId = traceId;
            this.SpanId = spanId;
        }

        public string OperationName { get; private set; }

        /// <summary>
        /// Globally unique. Every span in a trace shares this ID.
        /// </summary>
        public string TraceId { get; } // correlationid

        /// <summary>
        /// Unique within a trace. Each span within a trace contains a different ID.
        /// </summary>
        public string SpanId { get; }

        public DateTimeOffset? StartedDate { get; private set; }

        public DateTimeOffset? FinishedDate { get; private set; }

        public TimeSpan Duration =>
            this.FinishedDate.HasValue && this.StartedDate.HasValue ? this.FinishedDate.Value - this.StartedDate.Value : TimeSpan.Zero;

        public bool Failed { get; set; }

        public ISpan SetStartedDate(DateTimeOffset? date = null)
        {
            if(date.HasValue)
            {
                this.StartedDate = date;
            }
            else
            {
                this.StartedDate = DateTimeOffset.UtcNow;
            }

            return this;
        }

        public ISpan SetFinishedDate(DateTimeOffset? date = null)
        {
            if(date.HasValue)
            {
                this.FinishedDate = date;
            }
            else
            {
                this.FinishedDate = DateTimeOffset.UtcNow;
            }

            return this;
        }

        public ISpan SetOperationName(string operationName)
        {
            this.OperationName = operationName;
            return this;
        }

        public ISpan SetTag(string key, object value)
        {
            this.tags.AddOrUpdate(key, value);
            return this;
        }
    }
}
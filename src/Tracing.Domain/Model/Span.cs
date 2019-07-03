namespace Naos.Core.Tracing.Domain
{
    using System;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class Span : ISpan
    {
        private readonly DataDictionary tags = new DataDictionary();

        public Span(string traceId, string spanId, SpanKind kind = SpanKind.Internal, string parentSpanId = null)
        {
            this.TraceId = traceId;
            this.SpanId = spanId;
            this.Kind = kind;
            this.ParentSpanId = parentSpanId;
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

        public string ParentSpanId { get; }

        public SpanKind? Kind { get; }

        public SpanStatus? Status { get; private set; }

        public string StatusDescription { get; private set; }

        public DateTimeOffset? StartTime { get; private set; }

        public DateTimeOffset? EndTime { get; private set; }

        public TimeSpan Duration =>
            this.EndTime.HasValue && this.StartTime.HasValue ? this.EndTime.Value - this.StartTime.Value : TimeSpan.Zero;

        public ISpan Start(DateTimeOffset? date = null)
        {
            if(date.HasValue)
            {
                this.StartTime = date;
            }
            else
            {
                this.StartTime = DateTimeOffset.UtcNow;
            }

            return this;
        }

        public ISpan End(SpanStatus status = SpanStatus.Succeeded, string statusDescription = null, DateTimeOffset? date = null)
        {
            this.Status = status;
            this.StatusDescription = statusDescription;
            if(date.HasValue)
            {
                this.EndTime = date;
            }
            else
            {
                this.EndTime = DateTimeOffset.UtcNow;
            }

            return this;
        }

        public ISpan WithOperationName(string operationName)
        {
            this.OperationName = operationName;
            return this;
        }

        public ISpan WithTag(string key, object value)
        {
            this.tags.AddOrUpdate(key, value);
            return this;
        }

        public ISpan WithTags(DataDictionary tags)
        {
            foreach(var tag in tags.Safe())
            {
                this.tags.AddOrUpdate(tag.Key, tag.Value);
            }

            return this;
        }

        public ISpan SetStatus(SpanStatus status, string description = null)
        {
            this.Status = status;
            this.StatusDescription = description;
            return this;
        }
    }
}
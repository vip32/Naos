namespace Naos.Core.Tracing.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    [DebuggerDisplay("SpanId = {SpanId}, OperationName = {OperationName}")]
    public class Span : ISpan
    {
        private readonly List<SpanLogItem> logs = new List<SpanLogItem>();

        public Span(string traceId, string spanId, SpanKind kind = SpanKind.Internal, string parentSpanId = null)
        {
            this.TraceId = traceId;
            this.SpanId = spanId;
            this.Kind = kind;
            this.ParentSpanId = parentSpanId;
            this.LogKey = LogKeys.Tracing;
        }

        public string OperationName { get; private set; }

        /// <summary>
        /// Globally unique. Every span in a trace shares this ID.
        /// </summary>
        public string TraceId { get; } // correlationid

        /// <summary>
        /// Unique within a trace. Each span within a trace contains a different ID.
        /// </summary>
        public string SpanId { get; private set; }

        public string ParentSpanId { get; }

        public SpanKind? Kind { get; }

        public string LogKey { get; set; }

        public SpanStatus? Status { get; private set; }

        public bool? IsSampled { get; private set; }

        public string StatusDescription { get; private set; }

        public DateTimeOffset? StartTime { get; private set; }

        public DateTimeOffset? EndTime { get; private set; }

        public DataDictionary Tags { get; } = new DataDictionary();

        public IEnumerable<SpanLogItem> Logs
        {
            get { return this.logs; }
        }

        public TimeSpan Duration =>
            this.EndTime.HasValue && this.StartTime.HasValue ? this.EndTime.Value - this.StartTime.Value : TimeSpan.Zero;

        public ISpan Start(DateTimeOffset? date = null)
        {
            if (date.HasValue)
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
            // only set default status if none has been set already
            if (this.Status == SpanStatus.Transient || this.Status == null)
            {
                this.Status = status;
                this.StatusDescription = statusDescription;
            }

            if (date.HasValue)
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
            if (operationName.IsNullOrEmpty())
            {
                return this;
            }

            this.OperationName = operationName;
            return this;
        }

        public ISpan WithLogKey(string logKey)
        {
            if (logKey.IsNullOrEmpty())
            {
                return this;
            }

            this.LogKey = logKey;
            return this;
        }

        public ISpan SetSpanId(string spanId)
        {
            if (!spanId.IsNullOrEmpty())
            {
                this.SpanId = spanId;
            }

            return this;
        }

        public ISpan WithTag(string key, object value)
        {
            this.Tags.AddOrUpdate(key, value);
            return this;
        }

        public ISpan WithTags(DataDictionary tags)
        {
            foreach (var tag in tags.Safe())
            {
                this.Tags.AddOrUpdate(tag.Key, tag.Value);
            }

            return this;
        }

        public ISpan SetStatus(SpanStatus status, string description = null)
        {
            this.Status = status;
            this.StatusDescription = description;
            return this;
        }

        public ISpan SetSampled(bool? isSampled = true)
        {
            this.IsSampled = isSampled;
            return this;
        }

        public ISpan AddLog(string message)
        {
            return this.AddLog(SpanLogKey.Message, message);
        }

        public ISpan AddLog(string key, string message)
        {
            this.logs.Add(new SpanLogItem
            {
                Timestamp = DateTimeOffset.UtcNow,
                Key = key ?? SpanLogKey.Message,
                Message = message
            });

            return this;
        }
    }
}
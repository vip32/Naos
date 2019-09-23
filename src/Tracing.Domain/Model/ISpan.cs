namespace Naos.Tracing.Domain
{
    using System;
    using System.Collections.Generic;
    using Naos.Foundation.Domain;

    public interface ISpan
    {
        string OperationName { get; }

        string TraceId { get; }

        string SpanId { get; }

        string ParentSpanId { get; }

        SpanKind? Kind { get; }

        string LogKey { get; }

        SpanStatus? Status { get; }

        bool? IsSampled { get; }

        string StatusDescription { get; }

        DateTimeOffset? StartTime { get; }

        DateTimeOffset? EndTime { get; }

        DataDictionary Tags { get; }

        IEnumerable<SpanLogItem> Logs { get; }

        TimeSpan Duration { get; }

        ISpan Start(DateTimeOffset? date = null);

        ISpan End(SpanStatus status = SpanStatus.Succeeded, string statusDescription = null, DateTimeOffset? date = null);

        ISpan WithOperationName(string operationName);

        ISpan WithLogKey(string logKey);

        ISpan SetSpanId(string spanId);

        ISpan WithTag(string key, object value);

        ISpan WithTags(DataDictionary tags);

        ISpan SetStatus(SpanStatus status, string description = null);

        ISpan SetSampled(bool? isSampled = true);

        ISpan AddLog(string message);

        ISpan AddLog(string key, string message);
    }
}
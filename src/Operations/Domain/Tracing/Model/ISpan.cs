namespace Naos.Core.Operations.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public interface ISpan
    {
        string OperationName { get; }

        string TraceId { get; }

        string SpanId { get; }

        string ParentSpanId { get; }

        SpanKind? Kind { get; }

        SpanStatus? Status { get; }

        string StatusDescription { get; }

        DateTimeOffset? StartTime { get; }

        DateTimeOffset? EndTime { get;  }

        TimeSpan Duration { get; }

        ISpan Start(DateTimeOffset? date = null);

        ISpan End(SpanStatus status = SpanStatus.Succeeded, string statusDescription = null, DateTimeOffset? date = null);

        ISpan WithOperationName(string operationName);

        ISpan WithTag(string key, object value);

        ISpan WithTags(DataDictionary tags);

        ISpan SetStatus(SpanStatus status, string description = null);
    }
}
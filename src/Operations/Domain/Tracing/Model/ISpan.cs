namespace Naos.Core.Operations.Domain
{
    using System;

    public interface ISpan
    {
        string OperationName { get; }

        string SpanId { get; }

        string TraceId { get; }

        DateTimeOffset? StartedDate { get; }

        DateTimeOffset? FinishedDate { get;  }

        TimeSpan Duration { get; }

        bool Failed { get; set; }

        ISpan SetStartedDate(DateTimeOffset? date = null);

        ISpan SetFinishedDate(DateTimeOffset? date = null);

        ISpan SetTag(string key, object value);

        ISpan SetOperationName(string operationName);
    }
}
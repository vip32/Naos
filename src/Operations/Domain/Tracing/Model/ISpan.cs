namespace Naos.Core.Operations.Domain
{
    using System;

    public interface ISpan
    {
        string SpanId { get; }

        string TraceId { get; }

        DateTimeOffset StartedTimestamp { get; set; }

        DateTimeOffset FinishedTimestamp { get; set; }

        TimeSpan Duration { get; }

        bool Failed { get; set; }

        ISpan SetTag(string key, string value);
    }
}
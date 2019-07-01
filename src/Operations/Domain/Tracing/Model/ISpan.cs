namespace Naos.Core.Operations.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public interface ISpan
    {
        string OperationName { get; }

        string SpanId { get; }

        string TraceId { get; }

        DateTimeOffset? StartedDate { get; }

        DateTimeOffset? FinishedDate { get;  }

        TimeSpan Duration { get; }

        bool Failed { get; set; }

        ISpan Start(DateTimeOffset? date = null);

        ISpan Finish(DateTimeOffset? date = null);

        ISpan WithOperationName(string operationName);

        ISpan WithTag(string key, object value);

        ISpan WithTags(DataDictionary tags);
    }
}
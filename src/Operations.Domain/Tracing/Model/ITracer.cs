namespace Naos.Core.Operations.Domain
{
    using System;

    public interface ITracer
    {
        ISpan ActiveSpan { get; }

        IScopeManager ScopeManager { get; }

        ISpanBuilder BuildSpan(string operationName, SpanKind kind = SpanKind.Internal);

        void End(IScope scope = null, SpanStatus status = SpanStatus.Succeeded, string statusDescription = null);

        void Fail(IScope scope = null, Exception exception = null);
    }
}
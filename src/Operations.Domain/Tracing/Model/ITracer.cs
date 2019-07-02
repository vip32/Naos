namespace Naos.Core.Operations.Domain
{
    public interface ITracer
    {
        ISpan ActiveSpan { get; }

        IScopeManager ScopeManager { get; }

        ISpanBuilder BuildSpan(string operationName, SpanKind kind = SpanKind.Internal);

        void End(IScope scope = null, SpanStatus status = SpanStatus.Succeeded, string statusDescription = null);
    }
}
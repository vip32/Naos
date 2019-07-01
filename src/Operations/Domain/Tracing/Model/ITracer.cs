namespace Naos.Core.Operations.Domain
{
    public interface ITracer
    {
        ISpan ActiveSpan { get; }

        IScopeManager ScopeManager { get; }

        ISpanBuilder BuildSpan(string operationName);
    }
}
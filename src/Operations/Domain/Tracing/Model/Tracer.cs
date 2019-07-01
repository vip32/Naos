namespace Naos.Core.Operations.Domain
{
    public class Tracer : ITracer
    {
        public Tracer(IScopeManager scopeManager) // needs mediator
        {
            this.ScopeManager = scopeManager;
        }

        public ISpan ActiveSpan => this.ScopeManager?.Active?.Span; // use in outbound httpclient

        public IScopeManager ScopeManager { get; }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName, this.ActiveSpan);
        }
    }
}
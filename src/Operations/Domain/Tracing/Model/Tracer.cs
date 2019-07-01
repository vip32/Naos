namespace Naos.Core.Operations.Domain
{
    using EnsureThat;

    public class Tracer : ITracer
    {
        public Tracer(IScopeManager scopeManager) // needs correlationid (=traceid) get from ICorrelationContextAccessor
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.ScopeManager = scopeManager;
        }

        public ISpan ActiveSpan => this.ScopeManager.Active?.Span; // use in outbound httpclient

        public IScopeManager ScopeManager { get; }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName, this.ActiveSpan); // pass correlationid as traceid
        }
    }
}
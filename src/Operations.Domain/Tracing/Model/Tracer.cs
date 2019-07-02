namespace Naos.Core.Operations.Domain
{
    using System;
    using EnsureThat;
    using Naos.Foundation;

    public class Tracer : ITracer
    {
        public Tracer(IScopeManager scopeManager) // needs correlationid (=traceid) get from ICorrelationContextAccessor
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.ScopeManager = scopeManager;
        }

        public ISpan ActiveSpan => this.ScopeManager.Current?.Span; // use in outbound httpclient

        public IScopeManager ScopeManager { get; }

        public ISpanBuilder BuildSpan(string operationName, SpanKind kind = SpanKind.Internal)
        {
            return new SpanBuilder(this, operationName, kind, this.ActiveSpan); // pass correlationid as traceid
        }

        public void End(IScope scope = null, SpanStatus status = SpanStatus.Succeeded, string statusDescription = null)
        {
            scope ??= this.ScopeManager.Current;
            scope?.Span?.End(status, statusDescription);
            this.ScopeManager.Deactivate(scope);
        }

        public void Fail(IScope scope = null, Exception exception = null)
        {
            scope ??= this.ScopeManager.Current;
            scope?.Span?.End(SpanStatus.Failed, exception?.GetFullMessage());
            this.ScopeManager.Deactivate(scope);
        }
    }
}
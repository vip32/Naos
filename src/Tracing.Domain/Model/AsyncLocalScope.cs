namespace Naos.Core.Tracing.Domain
{
    using EnsureThat;

    public class AsyncLocalScope : IScope
    {
        private readonly AsyncLocalScopeManager scopeManager;
        private readonly bool finishOnDispose;
        private readonly IScope originalScope;

        public AsyncLocalScope(
            AsyncLocalScopeManager scopeManager,
            ISpan span,
            bool finishOnDispose = true)
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.scopeManager = scopeManager;
            this.Span = span;
            this.finishOnDispose = finishOnDispose;
            this.originalScope = scopeManager.Current;
            scopeManager.Current = this;
        }

        public ISpan Span { get; }

        public void Dispose()
        {
            if(this.finishOnDispose && this.Span != null)
            {
                this.Span.End();
                this.scopeManager.Deactivate(this); // publishes domainevent
            }

            this.scopeManager.Current = this.originalScope;
        }
    }
}
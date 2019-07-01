namespace Naos.Core.Operations.Domain
{
    using EnsureThat;

    public class AsyncLocalScope : IScope
    {
        private readonly AsyncLocalScopeManager scopeManager;
        private readonly bool finishOnDispose;
        private readonly IScope previousScope;

        public AsyncLocalScope(
            AsyncLocalScopeManager scopeManager,
            ISpan span,
            bool finishOnDispose = true)
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.scopeManager = scopeManager;
            this.Span = span;
            this.finishOnDispose = finishOnDispose;
            this.previousScope = scopeManager.Active;
            scopeManager.Active = this;
        }

        public ISpan Span { get; }

        public void Dispose()
        {
            if(this.finishOnDispose && this.Span != null)
            {
                this.Span.Finish();
                this.scopeManager.Finish(this.Span).Wait(); // publishes domainevent
            }

            this.scopeManager.Active = this.previousScope;
        }
    }
}
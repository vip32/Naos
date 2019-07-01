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
            ISpan wrappedSpan,
            bool finishOnDispose = true)
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.scopeManager = scopeManager;
            this.Span = wrappedSpan;
            this.finishOnDispose = finishOnDispose;
            this.previousScope = scopeManager.Active;
            scopeManager.Active = this;
        }

        public ISpan Span { get; }

        public void Dispose()
        {
            if(this.scopeManager.Active != this)
            {
                return; // shouldn't happen if users call methods in the expected order. ignore.
            }

            if(this.finishOnDispose)
            {
                this.Span.Finish();
                this.scopeManager.Finish(this.Span).Wait(); // publishes domainevent
            }

            this.scopeManager.Active = this.previousScope;
        }
    }
}
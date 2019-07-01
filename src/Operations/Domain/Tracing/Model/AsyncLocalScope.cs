namespace Naos.Core.Operations.Domain
{
    using System;

    public class AsyncLocalScope : IScope
    {
        private readonly AsyncLocalScopeManager scopeManager;
        private readonly ISpan wrappedSpan;
        private readonly bool finishOnDispose;
        private readonly IScope scopeToRestore;

        public AsyncLocalScope(
            AsyncLocalScopeManager scopeManager,
            ISpan wrappedSpan,
            bool finishOnDispose = true)
        {
            this.scopeManager = scopeManager;
            this.wrappedSpan = wrappedSpan;
            this.finishOnDispose = finishOnDispose;
            this.scopeToRestore = scopeManager.Active;
            scopeManager.Active = this;
        }

        public ISpan Span => this.wrappedSpan;

        public void Dispose()
        {
            if(this.scopeManager.Active != this)
            {
                // This shouldn't happen if users call methods in the expected order. ignore.
                return;
            }

            if(this.finishOnDispose)
            {
                this.Span.SetFinishedDate();
                this.scopeManager.Finish(this.Span).Wait(); // publishes domainevent
            }

            this.scopeManager.Active = this.scopeToRestore;
        }
    }
}
namespace Naos.Tracing.Domain
{
    using System;
    using System.Collections.Generic;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class AsyncLocalScope : IScope
    {
        private readonly AsyncLocalScopeManager scopeManager;
        private readonly bool finishOnDispose;
        private readonly IScope originalScope;
        private readonly IDisposable loggerScope;

        public AsyncLocalScope(
            AsyncLocalScopeManager scopeManager,
            ISpan span,
            ILogger logger,
            bool finishOnDispose = true)
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.scopeManager = scopeManager;
            this.Span = span;
            this.finishOnDispose = finishOnDispose;
            this.originalScope = scopeManager.Current;
            scopeManager.Current = this;
            this.loggerScope = logger?.BeginScope(new Dictionary<string, object>()
            {
                [LogPropertyKeys.TrackId] = span.SpanId // all log entries will be marked with current spanid
            });
        }

        public ISpan Span { get; }

        public void Dispose()
        {
            if (this.finishOnDispose && this.Span != null)
            {
                this.Span.End();
                this.scopeManager.Deactivate(this); // publishes domainevent
            }

            this.loggerScope?.Dispose();
            this.scopeManager.Current = this.originalScope;
        }
    }
}
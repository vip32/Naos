namespace Naos.Tracing.Domain
{
    using System.Threading;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class AsyncLocalScopeManager : IScopeManager
    {
        private readonly AsyncLocal<IScope> current = new AsyncLocal<IScope>();
        private readonly IMediator mediator;

        public AsyncLocalScopeManager(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public IScope Current
        {
            get => this.current.Value;
            set => this.current.Value = value;
        }

        public IScope Activate(ISpan span, ILogger logger, bool finishOnDispose = true)
        {
            if (this.mediator != null && this.IsSampled(span))
            {
                this.mediator.Publish(new SpanStartedDomainEvent(span)); // no await here
            }

            return new AsyncLocalScope(this, span, logger, finishOnDispose);
        }

        public void Deactivate(IScope scope)
        {
            if (this.mediator != null && this.IsSampled(scope.Span))
            {
                this.mediator.Publish(new SpanEndedDomainEvent(scope.Span));
            }
        }

        private bool IsSampled(ISpan span)
        {
            if(span == null)
            {
                return false;
            }

            return !span.IsSampled.HasValue || span.IsSampled == true;
        }
    }
}
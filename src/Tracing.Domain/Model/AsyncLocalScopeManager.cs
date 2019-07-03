namespace Naos.Core.Tracing.Domain
{
    using System.Threading;
    using MediatR;

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

        public IScope Activate(ISpan span, bool finishOnDispose = true)
        {
            if(this.mediator != null && span != null)
            {
                this.mediator.Publish(new SpanStartedDomainEvent(span)); // no await here
            }

            return new AsyncLocalScope(this, span, finishOnDispose);
        }

        public void Deactivate(IScope scope)
        {
            if(this.mediator != null && scope?.Span != null)
            {
                this.mediator.Publish(new SpanEndedDomainEvent(scope.Span));
            }
        }
    }
}
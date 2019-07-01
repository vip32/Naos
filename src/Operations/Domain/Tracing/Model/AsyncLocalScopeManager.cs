namespace Naos.Core.Operations.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Foundation;

    public class AsyncLocalScopeManager : IScopeManager
    {
        private readonly AsyncLocal<IScope> current = new AsyncLocal<IScope>();
        private readonly IMediator mediator;

        public AsyncLocalScopeManager(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public IScope Active
        {
            get => this.current.Value;
            set => this.current.Value = value;
        }

        public IScope Activate(ISpan span, bool finishSpanOnDispose = true)
        {
            return new AsyncLocalScope(this, span, finishSpanOnDispose);
        }

        public async Task Finish(ISpan span)
        {
            if(this.mediator != null)
            {
                await this.mediator.Publish(span).AnyContext(); // TODO: publish SpanFinishedDomainEvent
            }
        }
    }
}
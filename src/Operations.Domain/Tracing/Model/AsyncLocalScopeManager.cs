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

        public IScope Current
        {
            get => this.current.Value;
            set => this.current.Value = value;
        }

        public IScope Activate(ISpan span, bool finishOnDispose = true)
        {
            return new AsyncLocalScope(this, span, finishOnDispose);
        }

        public async Task Deactivate(ISpan span)
        {
            if(this.mediator != null && span != null)
            {
                await this.mediator.Publish(new SpanEndedDomainEvent(span)).AnyContext();
            }
        }
    }
}
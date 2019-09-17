namespace Naos.Core.Tracing.App
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class TracesConsoleCommandEventHandler : ConsoleCommandEventHandler<TracesConsoleCommand>
    {
        private readonly ILogTraceRepository repository;

        public TracesConsoleCommandEventHandler(ILogTraceRepository repository)
        {
            EnsureThat.EnsureArg.IsNotNull(repository, nameof(repository));

            this.repository = repository;
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<TracesConsoleCommand> request, CancellationToken cancellationToken)
        {
            if (request.Command.Recent)
            {
                var entities = await this.repository.FindAllAsync().AnyContext();
                var nodes = Node<LogTrace>.CreateTree(entities, l => l.SpanId, l => l.ParentSpanId, true)
                    .Where(n => !n.Children.IsNullOrEmpty()).ToList();

                nodes.RenderConsole();
            }

            return true;
        }
    }
}

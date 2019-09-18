namespace Naos.Core.Tracing.App
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Operations.App;
    using Naos.Core.RequestFiltering.App;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

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
                var filterContext = new FilterContext();
                LoggingFilterContext.Prepare(filterContext); // add some default criteria

                var entities = await this.repository.FindAllAsync(
                    filterContext.GetSpecifications<LogTrace>().Insert(new Specification<LogTrace>(t => t.TrackType == "trace")),
                    filterContext.GetFindOptions<LogTrace>()).AnyContext();
                var nodes = Node<LogTrace>.CreateTree(entities, l => l.SpanId, l => l.ParentSpanId, true)
                    .Where(n => !n.Children.IsNullOrEmpty()).ToList();

                nodes.RenderConsole();
            }

            return true;
        }
    }
}

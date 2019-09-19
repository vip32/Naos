namespace Naos.Core.Tracing.App
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
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
                var nodes = Node<LogTrace>.ToHierarchy(entities, l => l.SpanId, l => l.ParentSpanId, true);

                if(request.Command.Count > 0)
                {
                    nodes = nodes.Take(request.Command.Count);
                }

                await nodes.RenderConsole(
                    t => $"{t.Message} ({t.SpanId}/{t.ParentSpanId}) -> took {t.Duration.Humanize()}",
                    t => $"{t.Timestamp.ToUniversalTime():u} [{t.Kind?.ToUpper().Truncate(6, string.Empty)}] ",
                    orderBy: t => t.Ticks).AnyContext();
            }

            return true;
        }
    }
}

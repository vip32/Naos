namespace Naos.Core.Tracing.App
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Foundation;

    [Verb("traces", HelpText = "Shows trace information")]
    public class TracesConsoleCommand : IConsoleCommand
    {
        [Option('r', "recent", HelpText = "Show the recent traces", Required = false)]
        public bool Recent { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<TracesConsoleCommand>(this)).AnyContext();
        }
    }
}

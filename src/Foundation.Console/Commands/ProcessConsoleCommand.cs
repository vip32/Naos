namespace Naos.Foundation
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("process", HelpText = "Provides process information")]
    public class ProcessConsoleCommand : IConsoleCommand
    {
        [Option('c', "collect", HelpText = "Force GC collect")]
        public bool Collect { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<ProcessConsoleCommand>(this)).AnyContext();
        }
    }
}

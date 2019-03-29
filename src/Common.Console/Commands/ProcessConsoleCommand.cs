namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("process", HelpText = "Provides process information")]
    public class ProcessConsoleCommand : IConsoleCommand
    {
        [Option('m', "memory", HelpText = "Shows the memory usage (PrivateMemorySize)")]
        public bool Memory { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<ProcessConsoleCommand>(this)).AnyContext();
        }
    }
}

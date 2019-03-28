namespace Naos.Core.Queueing.App
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;

    [Verb("queueing", HelpText = "Queueing command")]
    public class QueueingConsoleCommand : IConsoleCommand
    {
        [Option("echo", HelpText = "Sends an echo item", Default = false)]
        public bool Echo { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<QueueingConsoleCommand>(this)).AnyContext();
        }
    }
}

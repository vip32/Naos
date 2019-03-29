namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("history", HelpText = "Show the command history")]
    public class HistoryConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<HistoryConsoleCommand>(this)).AnyContext();
        }
    }
}

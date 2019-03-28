namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("history", HelpText = "Show the command history")]
    public class ShowHistoryConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<ShowHistoryConsoleCommand>(this)).AnyContext();
        }
    }
}

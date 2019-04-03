namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("exit", HelpText = "Exit the interactive console")]
    public class ExitConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<ExitConsoleCommand>(this)).AnyContext();
        }
    }
}

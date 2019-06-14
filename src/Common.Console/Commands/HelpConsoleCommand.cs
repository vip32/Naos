namespace Naos.Foundation
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("?", HelpText = "Overview of available commands and general help")]
    public class HelpConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<HelpConsoleCommand>(this)).AnyContext();
        }
    }
}

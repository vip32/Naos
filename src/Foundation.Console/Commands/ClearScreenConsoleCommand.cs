namespace Naos.Foundation
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("cls", HelpText = "Clear the screen")]
    public class ClearScreenConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<ClearScreenConsoleCommand>(this)).AnyContext();
        }
    }
}

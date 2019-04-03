namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("logo", HelpText = "Show the logo")]
    public class LogoConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<LogoConsoleCommand>(this)).AnyContext();
        }
    }
}

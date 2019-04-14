namespace Naos.Core.Common.Web.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Core.Common.Console;

    [Verb("browser", HelpText = "Opens the browser")]
    public class OpenBrowserConsoleCommand : IConsoleCommand
    {
        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<OpenBrowserConsoleCommand>(this)).AnyContext();
        }
    }
}

namespace Naos.Foundation.Application
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("browser", HelpText = "Opens the browser")]
    public class OpenBrowserConsoleCommand : IConsoleCommand
    {
        [Option('l', "logs", HelpText = "Open the logs dashboard", Required = false)]
        public bool Logs { get; set; }

        [Option('t', "traces", HelpText = "Open the traces dashboard", Required = false)]
        public bool Traces { get; set; }

        [Option('j', "journal", HelpText = "Open the journal dashboard", Required = false)]
        public bool Journal { get; set; }

        [Option('s', "swagger", HelpText = "Open the journal dashboard", Required = false)]
        public bool Swagger { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<OpenBrowserConsoleCommand>(this)).AnyContext();
        }
    }
}

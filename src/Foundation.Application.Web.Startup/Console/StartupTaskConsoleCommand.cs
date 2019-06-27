namespace Naos.Foundation.Application
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("startuptasks", HelpText = "Startup tasks information")]
    public class StartupTaskConsoleCommand : IConsoleCommand
    {
        [Option('l', "list", HelpText = "Shows all registered startup tasks", Required = true)]
        public bool List { get; set; }

        // TODO: provide possibility to start a specific task (task.StartAsync())

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<StartupTaskConsoleCommand>(this)).AnyContext();
        }
    }
}

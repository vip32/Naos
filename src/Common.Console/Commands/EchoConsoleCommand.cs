namespace Naos.Core.Common.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;

    [Verb("echo", HelpText = "Echo a text message")]
    public class EchoConsoleCommand : IConsoleCommand
    {
        [Option('t', "text", HelpText = "Use the given text as the echo message", Required = true)]
        public string Text { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<EchoConsoleCommand>(this)).AnyContext();
        }
    }
}

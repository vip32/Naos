namespace Naos.Core.Messaging.App
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;

    [Verb("messaging", HelpText = "Messaging command")]
    public class MessagingConsoleCommand : IConsoleCommand
    {
        [Option("echo", HelpText = "Sends an echo message", Default = false)]
        public bool Echo { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<MessagingConsoleCommand>(this)).AnyContext();
        }
    }
}

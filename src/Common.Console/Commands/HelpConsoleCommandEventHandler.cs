namespace Naos.Core.Common.Console
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using Console = Colorful.Console;

    public class HelpConsoleCommandEventHandler : ConsoleCommandEventHandler<HelpConsoleCommand>
    {
        private readonly IEnumerable<IConsoleCommand> commands;

        public HelpConsoleCommandEventHandler(IEnumerable<IConsoleCommand> commands)
        {
            this.commands = commands.Safe();
        }

        public override Task<bool> Handle(ConsoleCommandEvent<HelpConsoleCommand> request, CancellationToken cancellationToken)
        {
            foreach (var command in this.commands)
            {
                Console.WriteLine($"{command.GetType().GetAttributeValue<VerbAttribute, string>(a => a.Name) ?? "?NAME?"} - {command.GetType().GetAttributeValue<VerbAttribute, string>(a => a.HelpText)}", Color.Gray); // ({command.GetType()})
            }

            return Task.FromResult(true);
        }
    }
}

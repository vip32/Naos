namespace Naos.Foundation.Application
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Console = Colorful.Console;

    public class StartupTaskConsoleCommandEventHandler : ConsoleCommandEventHandler<StartupTaskConsoleCommand>
    {
        private readonly ILogger<StartupTaskConsoleCommandEventHandler> logger;
        private readonly IEnumerable<IStartupTask> tasks;

        public StartupTaskConsoleCommandEventHandler(ILogger<StartupTaskConsoleCommandEventHandler> logger, IEnumerable<IStartupTask> tasks)
        {
            this.logger = logger;
            this.tasks = tasks;
        }

        public override Task<bool> Handle(ConsoleCommandEvent<StartupTaskConsoleCommand> request, CancellationToken cancellationToken)
        {
            if(request.Command.List)
            {
                foreach(var task in this.tasks.Safe())
                {
                    Console.WriteLine($"- {task.GetType().PrettyName()}", Color.Gray);
                }
            }

            return Task.FromResult(true);
        }
    }
}

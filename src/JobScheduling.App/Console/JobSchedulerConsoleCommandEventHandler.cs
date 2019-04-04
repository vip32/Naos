namespace Naos.Core.JobScheduling.App.Console
{
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;
    using Naos.Core.JobScheduling.Domain;
    using Console = Colorful.Console;

    public class JobSchedulerConsoleCommandEventHandler : ConsoleCommandEventHandler<JobSchedulerConsoleCommand>
    {
        private readonly ILogger<JobSchedulerConsoleCommandEventHandler> logger;
        private readonly IJobScheduler jobScheduler;

        public JobSchedulerConsoleCommandEventHandler(ILogger<JobSchedulerConsoleCommandEventHandler> logger, IJobScheduler jobScheduler)
        {
            this.logger = logger;
            this.jobScheduler = jobScheduler;
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<JobSchedulerConsoleCommand> request, CancellationToken cancellationToken)
        {
            if(request.Command.Enable)
            {
                Console.WriteLine("\r\nenable", Color.LimeGreen);
                this.logger.LogInformation($"{LogEventKeys.JobScheduling:l} enabling");
                this.jobScheduler.Options.SetEnabled();
            }

            if(request.Command.Disable)
            {
                Console.WriteLine("\r\ndisable", Color.LimeGreen);
                this.logger.LogInformation($"{LogEventKeys.JobScheduling:l} disabling");
                this.jobScheduler.Options.SetEnabled(false);
            }

            if(request.Command.List)
            {
                foreach(var key in this.jobScheduler.Options.Registrations.Keys.Safe())
                {
                    Console.WriteLine($"[{key.Identifier}] {key.Key} ({key.Cron})");
                }
            }

            if(!request.Command.Trigger.IsNullOrEmpty())
            {
                Console.WriteLine($"\r\nstart job {request.Command.Trigger}", Color.LimeGreen);

                await this.jobScheduler.TriggerAsync(request.Command.Trigger).AnyContext();
            }

            var text = $@"status:
-enabled={this.jobScheduler.Options.Enabled}
-running={this.jobScheduler.IsRunning}";

            await Task.Run(() => System.Console.WriteLine(text));
            return true;
        }
    }
}

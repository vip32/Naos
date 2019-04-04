namespace Naos.Core.JobScheduling.App.Console
{
    using System.Threading.Tasks;
    using CommandLine;
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;

    [Verb("jobscheduler", HelpText = "Job scheduler command")]
    public class JobSchedulerConsoleCommand : IConsoleCommand
    {
        [Option("enable", HelpText = "Enables the current job scheduler", Default = false)]
        public bool Enable { get; set; }

        [Option("disable", HelpText = "Disables the current job scheduler", Default = false)]
        public bool Disable { get; set; }

        [Option("trigger", HelpText = "Triggers a job, should contain the job key")]
        public string Trigger { get; set; }

        [Option("list", HelpText = "Shows list of registered jobs", Default = false)]
        public bool List { get; set; }

        //[Option("repeat", HelpText = "Repeat the action")]
        //public int Repeat { get; set; }

        public async Task<bool> SendAsync(IMediator mediator)
        {
            return await mediator.Send(new ConsoleCommandEvent<JobSchedulerConsoleCommand>(this)).AnyContext();
        }
    }
}

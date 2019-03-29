namespace Naos.Core.Common.Console
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Console = Colorful.Console;

    public class ProcessConsoleCommandEventHandler : ConsoleCommandEventHandler<ProcessConsoleCommand>
    {
        private readonly ILogger<ProcessConsoleCommandEventHandler> logger;

        public ProcessConsoleCommandEventHandler(ILogger<ProcessConsoleCommandEventHandler> logger)
        {
            this.logger = logger;
        }

        public override Task<bool> Handle(ConsoleCommandEvent<ProcessConsoleCommand> request, CancellationToken cancellationToken)
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLine($"PID: {process.Id}", Color.Gray);
            Console.WriteLine($"Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}", Color.Gray);
            Console.WriteLine($"Is64BitProcess: {Environment.Is64BitProcess}", Color.Gray);

            //"PhysicalPath", HostingEnvironment.ApplicationPhysicalPath);
            //"Machine", Environment.MachineName);
            //"ProcessorCount", Environment.ProcessorCount);
            //"ThreadId", Environment.CurrentManagedThreadId);
            //"WorkingSetMemory", Environment.WorkingSet.Bytes().ToString("MB"));
            //"AvailableMemory", $"{new PerformanceCounter("Memory", "Available MBytes").NextValue()} MB");
            //"Cpu", $"{new PerformanceCounter("Processor", "% Processor Time", "_Total").NextValue()}%");
            //"Is64BitProcess", Environment.Is64BitProcess);
            //"Domain", Environment.UserDomainName);

            if (request.Command.Memory)
            {
                Console.WriteLine(process.PrivateMemorySize64.Bytes().ToString("#.##"), Color.Gray);
            }

            //this.logger.LogInformation(text);
            return Task.FromResult(true);
        }
    }
}

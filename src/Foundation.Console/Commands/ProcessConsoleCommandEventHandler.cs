namespace Naos.Foundation
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
            Console.WriteLine($"Memory used: {process.PrivateMemorySize64.Bytes():#.##}", Color.Gray);
            Console.WriteLine($"GC Gen-0: {GC.CollectionCount(0)} (fast/blocking)", Color.Gray);
            Console.WriteLine($"GC Gen-1: {GC.CollectionCount(1)} (fast/blocking)", Color.Gray);
            Console.WriteLine($"GC Gen-2: {GC.CollectionCount(2)} (slow/nonblocking)", Color.Gray);
            Console.WriteLine($"Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}", Color.Gray);
            Console.WriteLine($"Is64BitProcess: {Environment.Is64BitProcess}", Color.Gray);

            if (request.Command.Collect)
            {
                Console.WriteLine("GC collect", Color.Gray);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            //"PhysicalPath", HostingEnvironment.ApplicationPhysicalPath);
            //"Machine", Environment.MachineName);
            //"ProcessorCount", Environment.ProcessorCount);
            //"ThreadId", Environment.CurrentManagedThreadId);
            //"WorkingSetMemory", Environment.WorkingSet.Bytes().ToString("MB"));
            //"AvailableMemory", $"{new PerformanceCounter("Memory", "Available MBytes").NextValue()} MB");
            //"Cpu", $"{new PerformanceCounter("Processor", "% Processor Time", "_Total").NextValue()}%");
            //"Is64BitProcess", Environment.Is64BitProcess);
            //"Domain", Environment.UserDomainName);

            //this.logger.LogInformation(text);
            return Task.FromResult(true);
        }
    }
}

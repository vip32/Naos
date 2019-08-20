namespace Naos.Foundation
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class SystemMetricsClient
    {
        public SystemMemoryMetrics GetMemoryMetrics() =>
            this.IsUnix() ? this.GetMemoryMetricsUnix() : this.GetMemoryMetricsWindows();

        public SystemCpuMetrics GetCpuMetrics() =>
            this.IsUnix() ? this.GetCpuMetricsUnix() : this.GetCpuMetricsWindows();

        private bool IsUnix() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                   || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private SystemMemoryMetrics GetMemoryMetricsWindows()
        {
            var output = string.Empty;
            var info = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split('\n');
            var freeMemoryParts = lines[0].Split('=').Safe();
            var totalMemoryParts = lines[1].Split('=').Safe();

            var metrics = new SystemMemoryMetrics
            {
                Total = Math.Round(double.Parse(totalMemoryParts.Last()) / 1024, 0),
                Free = Math.Round(double.Parse(freeMemoryParts.Last()) / 1024, 0)
            };
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private SystemCpuMetrics GetCpuMetricsWindows()
        {
            var output = string.Empty;
            var info = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "CPU get LoadPercentage /Value", // wmic CPU list full, wmic diskdrive get status /Value
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split('\n');
            var loadPercentageParts = lines[0].Split('=').Safe();

            return new SystemCpuMetrics
            {
                LoadPercentage = Math.Round(double.Parse(loadPercentageParts.Last()), 0),
            };
        }

        private SystemMemoryMetrics GetMemoryMetricsUnix()
        {
            var output = string.Empty;
            var info = new ProcessStartInfo("free -m")
            {
                FileName = "/bin/bash",
                Arguments = "-c \"free -m\"", // WARN: LoadPercentage (cpu) not available
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split('\n');
            var memory = lines[1].Split(' ').Safe().ToArray();

            return new SystemMemoryMetrics
            {
                Total = double.Parse(memory[1]),
                Used = double.Parse(memory[2]),
                Free = double.Parse(memory[3])
            };
        }

        private SystemCpuMetrics GetCpuMetricsUnix()
        {
            var output = string.Empty;
            var info = new ProcessStartInfo("free -m")
            {
                FileName = "/bin/bash",
                Arguments = "-c \"free -m\"", // WARN: LoadPercentage (cpu) not available
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            //var lines = output.Split('\n');
            //var memory = lines[1].Split(' ').Safe().ToArray();

            return new SystemCpuMetrics
            {
                //Load = ??
            };
        }
    }
}

namespace Naos.Core.Commands.Operations.Serilog
{
    using System.IO;

    public class LogFileConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string Folder { get; set; } = Path.GetTempPath();

        public string SubFolder { get; set; } = "naos_operations";

        public string File { get; set; } = "naos.log";

        public string OutputTemplate { get; set; }

        public int FileSizeLimitBytes { get; set; } = 1_000_000;

        public bool RollOnFileSizeLimit { get; set; } = true;

        public string RollingInterval { get; set; } = "Infinite"; // Year, Month, Day, Hour, Minute

        public bool Shared { get; set; }

        public int FlushToDiskIntervalSeconds { get; set; } = 1;
    }
}

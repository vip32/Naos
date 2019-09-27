namespace Naos.Operations.App
{
    using System.IO;

    public class DiagnosticsLogStreamLoggingConfiguration
    {
        public DiagnosticsLogStreamLoggingConfiguration()
        {
            this.File = @"D:\home\LogFiles\Application\naos.log";
            this.FileSizeLimitBytes = 1_000_000;
            this.RollOnFileSizeLimit = true;
            this.Shared = true;
            this.FlushToDiskIntervalSeconds = 1;
        }

        public bool Enabled { get; set; } = true;

        public string Folder { get; set; } = Path.GetTempPath();

        public string SubFolder { get; set; } = "naos_operations";

        public string File { get; set; }

        public string OutputTemplate { get; set; }

        public long? FileSizeLimitBytes { get; set; } //= 1000000; // 1000000 = 1 MB

        public bool RollOnFileSizeLimit { get; set; } = false;

        public string RollingInterval { get; set; } = "Infinite"; // Year, Month, Day, Hour, Minute, Infinite

        public bool Shared { get; set; } = false; // prevent multiple log files, true means slower due to write locks

        public int? FlushToDiskIntervalSeconds { get; set; }
    }
}

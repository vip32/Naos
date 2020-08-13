namespace Naos.Operations.Application
{
    using System.IO;

    public class FileLoggingConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string Folder { get; set; } = Path.GetTempPath();

        public string SubFolder { get; set; } = "naos_operations";

        public string File { get; set; }

        public string OutputTemplate { get; set; }

        public long? FileSizeLimitBytes { get; set; } //= 1000000; // 1000000 = 1 MB

        public bool RollOnFileSizeLimit { get; set; }

        public string RollingInterval { get; set; } = "Infinite"; // Year, Month, Day, Hour, Minute, Infinite

        public bool Shared { get; set; } // prevent multiple log files, true means slower due to write locks

        public int? FlushToDiskIntervalSeconds { get; set; }
    }
}
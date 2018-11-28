namespace Naos.Core.App.Operations.Serilog
{
    public class LogFileConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string FileName { get; set; } = "naos.log";

        public string OutputTemplate { get; set; }

        public int FileSizeLimitBytes { get; set; } = 1_000_000;

        public bool RollOnFileSizeLimit { get; set; } = true;

        public string RollingInterval { get; set; } = "Infinite"; // Year, Month, Day, Hour, Minute

        public bool Shared { get; set; }

        public int FlushToDiskIntervalSeconds { get; set; } = 1;
    }
}

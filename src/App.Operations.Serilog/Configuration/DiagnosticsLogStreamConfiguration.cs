namespace Naos.Core.App.Operations.Serilog
{
    public class DiagnosticsLogStreamConfiguration : LogFileConfiguration
    {
        public DiagnosticsLogStreamConfiguration()
        {
            this.FileName = @"D:\home\LogFiles\Application\naos.log";
            this.FileSizeLimitBytes = 1_000_000;
            this.RollOnFileSizeLimit = true;
            this.Shared = true;
            this.FlushToDiskIntervalSeconds = 1;
        }
    }
}

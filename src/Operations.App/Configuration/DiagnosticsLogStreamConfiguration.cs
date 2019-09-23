namespace Naos.Operations.App
{
    public class DiagnosticsLogStreamConfiguration : LogFileConfiguration
    {
        public DiagnosticsLogStreamConfiguration()
        {
            this.File = @"D:\home\LogFiles\Application\naos.log";
            this.FileSizeLimitBytes = 1_000_000;
            this.RollOnFileSizeLimit = true;
            this.Shared = true;
            this.FlushToDiskIntervalSeconds = 1;
        }
    }
}

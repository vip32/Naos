namespace Naos.Tracing.Domain
{
    using Microsoft.Extensions.Logging;

    public class LoggerTraceAdapter : ITraceAdapter
    {
        private readonly ILogger<LoggerTraceAdapter> logger;

        public LoggerTraceAdapter(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<LoggerTraceAdapter>();
        }

        // TODO: store the trace event by using the logger (with LogAnalytics sink)
    }
}

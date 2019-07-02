namespace Naos.Core.Operations.Infrastructure.Azure
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;

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

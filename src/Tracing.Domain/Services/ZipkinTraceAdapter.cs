namespace Naos.Core.Tracing.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;

    public class ZipkinTraceAdapter : ITraceAdapter // TODO: move to infrastructure
    {
        private readonly ILogger<LoggerTraceAdapter> logger;

        public ZipkinTraceAdapter(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<LoggerTraceAdapter>();
        }

        // TODO: store the trace event in zipkin https://github.com/openzipkin/zipkin4net
    }
}

namespace Naos.Core.Operations.Infrastructure.Azure
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.Operations.Domain;

    public class ZipkinTraceAdapter : ITraceAdapter
    {
        private readonly ILogger<LoggerTraceAdapter> logger;

        public ZipkinTraceAdapter(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<LoggerTraceAdapter>();
        }

        // TODO: store the trace event in zipkin https://github.com/openzipkin/zipkin4net
    }
}

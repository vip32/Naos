namespace Naos.Tracing.Infrastructure.Azure
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Infrastructure;
    using Naos.Foundation.Infrastructure.Azure;
    using Naos.Tracing.Domain;

    public class LogAnalyticsLogTraceRepository : LogAnalyticsRepository<LogTrace>, ILogTraceRepository
    {
        public LogAnalyticsLogTraceRepository(
            ILoggerFactory loggerFactory,
            HttpClient httpClient,
            LogAnalyticsConfiguration configuration,
            string accessToken)
            : base(
                loggerFactory,
                httpClient,
                configuration,
                accessToken,
                new[]
                {
                    new LogAnalyticsEntityMap("TrackId", LogPropertyKeys.TrackId, $"LogProperties_{LogPropertyKeys.TrackId}_s"),
                    new LogAnalyticsEntityMap("OperationName", LogPropertyKeys.TrackName, $"LogProperties_{LogPropertyKeys.TrackName}_s"),
                    new LogAnalyticsEntityMap("TraceId", LogPropertyKeys.CorrelationId, $"LogProperties_{LogPropertyKeys.CorrelationId}_s"),
                    new LogAnalyticsEntityMap("SpanId", LogPropertyKeys.TrackId, $"LogProperties_{LogPropertyKeys.TrackId}_s"),
                    new LogAnalyticsEntityMap("ParentSpanId", LogPropertyKeys.TrackParentId, $"LogProperties_{LogPropertyKeys.TrackParentId}_s"),
                    new LogAnalyticsEntityMap("Kind", LogPropertyKeys.TrackKind, $"LogProperties_{LogPropertyKeys.TrackKind}_s"),
                    new LogAnalyticsEntityMap("Status", LogPropertyKeys.TrackStatus, $"LogProperties_{LogPropertyKeys.TrackStatus}_s"),
                    new LogAnalyticsEntityMap("StatusDescription", LogPropertyKeys.TrackStatusDescription, $"LogProperties_{LogPropertyKeys.TrackStatusDescription}_s"),
                    new LogAnalyticsEntityMap("StartTime", LogPropertyKeys.TrackStartTime, $"LogProperties_{LogPropertyKeys.TrackStartTime}_t"),
                    new LogAnalyticsEntityMap("EndTime", LogPropertyKeys.TrackEndTime, $"LogProperties_{LogPropertyKeys.TrackEndTime}_t")
                })
        {
        }
    }
}

namespace Naos.Foundation.Infrastructure.Azure
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Operations.Domain;

    public class LogAnalyticsEntityMap
    {
        public LogAnalyticsEntityMap(string entityProperty, string dtoProperty, string dtoPropertyFull)
        {
            this.EntityProperty = entityProperty;
            this.DtoProperty = dtoProperty;
            this.DtoPropertyFull = dtoPropertyFull;
        }

        public string EntityProperty { get; set; }

        public string DtoProperty { get; set; }

        public string DtoPropertyFull { get; set; }

        public static IEnumerable<LogAnalyticsEntityMap> CreateDefault() =>
            new[]
            {
                new LogAnalyticsEntityMap(nameof(LogEvent.Environment), LogPropertyKeys.Environment, $"LogProperties_{LogPropertyKeys.Environment}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.Level), "LogLevel", "LogLevel_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.Ticks), LogPropertyKeys.Ticks, $"LogProperties_{LogPropertyKeys.Ticks}_d"), // .To<long>()
                new LogAnalyticsEntityMap(nameof(LogEvent.TrackType), LogPropertyKeys.TrackType, $"LogProperties_{LogPropertyKeys.TrackType}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.TrackId), LogPropertyKeys.TrackId, $"LogProperties_{LogPropertyKeys.TrackId}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.Id), LogPropertyKeys.Id, $"LogProperties_{LogPropertyKeys.Id}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.CorrelationId), LogPropertyKeys.CorrelationId, $"LogProperties_{LogPropertyKeys.CorrelationId}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.Key), LogPropertyKeys.LogKey, $"LogProperties_{LogPropertyKeys.LogKey}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.Message), "LogMessage", "LogMessage_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.Timestamp), "Timestamp", "Timestamp"), // to DateTime
                new LogAnalyticsEntityMap(nameof(LogEvent.SourceContext), "SourceContext", "SourceContext"),
                new LogAnalyticsEntityMap(nameof(LogEvent.ServiceName), LogPropertyKeys.ServiceName, $"LogProperties_{LogPropertyKeys.ServiceName}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.ServiceProduct), LogPropertyKeys.ServiceProduct, $"LogProperties_{LogPropertyKeys.ServiceProduct}_s"),
                new LogAnalyticsEntityMap(nameof(LogEvent.ServiceCapability), LogPropertyKeys.ServiceCapability, $"LogProperties_{LogPropertyKeys.ServiceCapability}_s"),
            };
    }
}

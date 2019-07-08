namespace Naos.Foundation.Infrastructure.Azure
{
    using System.Collections.Generic;
    using Naos.Foundation;

    public class LogAnalyticsEntityMap
    {
        public LogAnalyticsEntityMap(string sourceProperty, string targetProperty, string targetPropertyFull)
        {
            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;
            this.TargetPropertyFull = targetPropertyFull;
        }

        public string SourceProperty { get; set; }

        public string TargetProperty { get; set; }

        public string TargetPropertyFull { get; set; }

        public static IEnumerable<LogAnalyticsEntityMap> CreateDefault() =>
            new[]
            {
                new LogAnalyticsEntityMap("Environment", LogPropertyKeys.Environment, $"LogProperties_{LogPropertyKeys.Environment}_s"),
                new LogAnalyticsEntityMap("Level", "LogLevel", "LogLevel_s"),
                new LogAnalyticsEntityMap("Ticks", LogPropertyKeys.Ticks, $"LogProperties_{LogPropertyKeys.Ticks}_d"), // .To<long>()
                new LogAnalyticsEntityMap("TrackType", LogPropertyKeys.TrackType, $"LogProperties_{LogPropertyKeys.TrackType}_s"),
                new LogAnalyticsEntityMap("Id", LogPropertyKeys.Id, $"LogProperties_{LogPropertyKeys.Id}_s"),
                new LogAnalyticsEntityMap("CorrelationId", LogPropertyKeys.CorrelationId, $"LogProperties_{LogPropertyKeys.CorrelationId}_s"),
                new LogAnalyticsEntityMap("Key", LogPropertyKeys.LogKey, $"LogProperties_{LogPropertyKeys.LogKey}_s"),
                new LogAnalyticsEntityMap("Message", "LogMessage", "LogMessage"),
                new LogAnalyticsEntityMap("Timestamp", "Timestamp", "Timestamp"), // to DateTime
                new LogAnalyticsEntityMap("SourceContext", "SourceContext", "SourceContext"),
                new LogAnalyticsEntityMap("ServiceName", LogPropertyKeys.ServiceName, $"LogProperties_{LogPropertyKeys.ServiceName}_s"),
                new LogAnalyticsEntityMap("ServiceProduct", LogPropertyKeys.ServiceProduct, $"LogProperties_{LogPropertyKeys.ServiceProduct}_s"),
                new LogAnalyticsEntityMap("ServiceCapability", LogPropertyKeys.ServiceCapability, $"LogProperties_{LogPropertyKeys.ServiceCapability}_s"),
            };
    }
}

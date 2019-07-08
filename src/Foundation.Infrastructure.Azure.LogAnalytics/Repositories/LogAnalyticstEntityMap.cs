namespace Naos.Foundation.Infrastructure.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using FastMember;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class LogAnalyticstEntityMap
    {
        public LogAnalyticstEntityMap(string sourceProperty, string targetProperty, string targetPropertyFull)
        {
            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;
            this.TargetPropertyFull = targetPropertyFull;
        }

        public string SourceProperty { get; set; }

        public string TargetProperty { get; set; }

        public string TargetPropertyFull { get; set; }

        public static IEnumerable<LogAnalyticstEntityMap> CreateDefault() =>
            new[]
            {
                new LogAnalyticstEntityMap("Environment", LogPropertyKeys.Environment, $"LogProperties_{LogPropertyKeys.Environment}_s"),
                new LogAnalyticstEntityMap("Level", "LogLevel", "LogLevel_s"),
                new LogAnalyticstEntityMap("Ticks", LogPropertyKeys.Ticks, $"LogProperties_{LogPropertyKeys.Ticks}_d"), // .To<long>()
                new LogAnalyticstEntityMap("TrackType", LogPropertyKeys.TrackType, $"LogProperties_{LogPropertyKeys.TrackType}_s"),
                new LogAnalyticstEntityMap("Id", LogPropertyKeys.Id, $"LogProperties_{LogPropertyKeys.Id}_s"),
                new LogAnalyticstEntityMap("CorrelationId", LogPropertyKeys.CorrelationId, $"LogProperties_{LogPropertyKeys.CorrelationId}_s"),
                new LogAnalyticstEntityMap("Key", LogPropertyKeys.LogKey, $"LogProperties_{LogPropertyKeys.LogKey}_s"),
                new LogAnalyticstEntityMap("Message", "LogMessage", "LogMessage"),
                new LogAnalyticstEntityMap("Timestamp", "Timestamp", "Timestamp"), // to DateTime
                new LogAnalyticstEntityMap("SourceContext", "SourceContext", "SourceContext"),
                new LogAnalyticstEntityMap("ServiceName", LogPropertyKeys.ServiceName, $"LogProperties_{LogPropertyKeys.ServiceName}_s"),
                new LogAnalyticstEntityMap("ServiceProduct", LogPropertyKeys.ServiceProduct, $"LogProperties_{LogPropertyKeys.ServiceProduct}_s"),
                new LogAnalyticstEntityMap("ServiceCapability", LogPropertyKeys.ServiceCapability, $"LogProperties_{LogPropertyKeys.ServiceCapability}_s"),
            };
    }
}

namespace Naos.Operations.Application.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Naos.Foundation;
    using Newtonsoft.Json.Linq;

    public static class HealthReportResponseWriter
    {
        public static Task Write(HttpContext httpContext, HealthReport report)
        {
            //return Task.Run(() => httpContext.Response.WriteJson(MapJsonReport(report)));
            return Task.Run(() => httpContext.Response.WriteJson(MapReport(report)));
        }

        public static JObject MapJsonReport(HealthReport report)
        {
            if(report == null)
            {
                return new JObject();
            }

            return new JObject(
                new JProperty("status", report.Status.ToString()),
                new JProperty("duration", report.TotalDuration),
                new JProperty("took", report.TotalDuration.Humanize()),
                new JProperty("timestamp", DateTime.UtcNow.ToString("o")),
                new JProperty("entries", new JObject(report.Entries?.Select(pair =>
                    new JProperty(pair.Key?.ToLower(), new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("duration", pair.Value.Duration),
                        new JProperty("took", pair.Value.Duration.Humanize()),
                        new JProperty("error", pair.Value.Exception != null ? $"[{pair.Value.Exception.GetType().Name}] {pair.Value.Exception.Message}" : null),
                        new JProperty("data", new JObject(pair.Value.Data.Select(p => new JProperty(p.Key, p.Value))))))))));
        }

        public static NaosHealthReport MapReport(HealthReport report)
        {
            if (report == null)
            {
                return new NaosHealthReport();
            }

            var result = new NaosHealthReport
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Took = report.TotalDuration.Humanize(),
                Timestamp = DateTime.UtcNow.ToString("o"),
                Entries = new Dictionary<string, NaosHealthReportEntry>()
            };

            foreach(var entry in report.Entries.Safe())
            {
                if(entry.Key == null)
                {
                    continue;
                }

                result.Entries.AddOrUpdate(entry.Key, new NaosHealthReportEntry
                {
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Took = entry.Value.Duration.Humanize(),
                    Error = entry.Value.Exception != null ? $"[{entry.Value.Exception.GetType().Name}] {entry.Value.Exception.Message}" : null,
                    Data = entry.Value.Data
                });
            }

            return result;
        }
    }
}

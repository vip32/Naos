namespace Naos.Operations.Application.Web
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Newtonsoft.Json.Linq;

    public static class HealthReportResponseWriter
    {
        public static Task Write(HttpContext httpContext, HealthReport report)
        {
            return Task.Run(() => httpContext.Response.WriteJson(Generate(report)));
        }

        public static JObject Generate(HealthReport report)
        {
            return new JObject(
                new JProperty("status", report.Status.ToString()),
                new JProperty("duration", report.TotalDuration),
                new JProperty("entries", new JObject(report.Entries.Select(pair =>
                    new JProperty(pair.Key?.ToLower(), new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("duration", pair.Value.Duration),
                        new JProperty("error", pair.Value.Exception != null ? $"[{pair.Value.Exception.GetType().Name}] {pair.Value.Exception.Message}" : null),
                        new JProperty("data", new JObject(pair.Value.Data.Select(p => new JProperty(p.Key, p.Value))))))))));
        }
    }
}

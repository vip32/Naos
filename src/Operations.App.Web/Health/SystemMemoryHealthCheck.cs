namespace Naos.Core.Operations.App.Web
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Naos.Foundation;

    public class SystemMemoryHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var client = new SystemMetricsClient();
            var metrics = client.GetMemoryMetrics();
            var percentUsed = 100 * metrics.Used / metrics.Total;

            var status = HealthStatus.Healthy;
            if (percentUsed > 80)
            {
                status = HealthStatus.Degraded;
            }
            else if (percentUsed > 90)
            {
                status = HealthStatus.Unhealthy;
            }

            var data = new Dictionary<string, object>
            {
                { "Total", metrics.Total },
                { "Used", metrics.Used },
                { "Free", metrics.Free }
            };

            var result = new HealthCheckResult(status, null, null, data);

            return await Task.FromResult(result).AnyContext();
        }
    }
}

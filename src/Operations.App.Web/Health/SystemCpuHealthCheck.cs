namespace Naos.Operations.App.Web
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Naos.Foundation;

    public class SystemCpuHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var client = new SystemMetricsClient();
            var metrics = client.GetCpuMetrics();

            var status = HealthStatus.Healthy;
            if (metrics.LoadPercentage > 80)
            {
                status = HealthStatus.Degraded;
            }
            else if (metrics.LoadPercentage > 90)
            {
                status = HealthStatus.Unhealthy;
            }

            var data = new Dictionary<string, object>
            {
                { "load", metrics.LoadPercentage }
            };

            var result = new HealthCheckResult(status, null, null, data);

            return await Task.FromResult(result).AnyContext();
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using HealthChecks.Uris;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Naos.Core.Common;
    using Naos.Core.ServiceDiscovery.App;

    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddServiceDiscoveryProxy<T>(
            this IHealthChecksBuilder builder,
            string name = null,
            string route = "echo",
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
            where T : ServiceDiscoveryClient
        {
            name = name ?? typeof(T).Name;
            if (name.EndsWith("Client", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Replace("Client", "-client", StringComparison.OrdinalIgnoreCase);
            }

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => CreateHealthCheck<T>(sp, name, route),
                failureStatus,
                tags));
        }

        private static UriHealthCheck CreateHealthCheck<T>(IServiceProvider sp, string name, string route)
            where T : ServiceDiscoveryClient
        {
            var proxy = sp.GetRequiredService<T>();
            if(proxy == null)
            {
                throw new NaosException($"Health: ServiceDiscovery client '{typeof(T)}' not found, please add with service.AddHttpClient<ServiceDiscoveryProxy>()");
            }

            var options = new UriHealthCheckOptions();
            var address = proxy.HttpClient?.BaseAddress?.ToString();
            if (address.IsNullOrEmpty())
            {
                throw new NaosException($"Health: ServiceDiscovery client '{typeof(T)}' address not found, registration inactive (due to health) or missing from registry?");
            }

            options.AddUri(new Uri(new Uri(address.Remove("router")), route));

            //var httpClientFactory = sp.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            return new UriHealthCheck(options, () => proxy.HttpClient); // httpClientFactory.CreateClient(name)
        }
    }
}

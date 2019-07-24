namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using HealthChecks.Uris;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Foundation;

    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddServiceDiscoveryClient<T>(
            this IHealthChecksBuilder builder,
            string name = null,
            string route = "api/echo",
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
            where T : ServiceDiscoveryClient
        {
            name = name ?? typeof(T).Name;
            if(name.EndsWith("Client", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Replace("Client", "-serviceclient", StringComparison.OrdinalIgnoreCase);
            }

            return builder.Add(
                new HealthCheckRegistration(
                    name,
                    sp => CreateHealthCheck<T>(sp, route),
                    failureStatus,
                    tags));
        }

        private static UriHealthCheck CreateHealthCheck<T>(IServiceProvider sp, string route)
            where T : ServiceDiscoveryClient
        {
            var serviceClient = sp.GetRequiredService<T>();
            if(serviceClient == null)
            {
                throw new NaosException($"Health: ServiceDiscovery client '{typeof(T)}' not found, please add with service.AddHttpClient<ServiceDiscoveryProxy>()");
            }

            var options = new UriHealthCheckOptions();
            var address = serviceClient.HttpClient?.BaseAddress?.ToString(); //.SubstringTill("servicediscovery");
            if(address.IsNullOrEmpty())
            {
                throw new NaosException($"Health: ServiceDiscovery client '{typeof(T)}' address not found, registration inactive (due to health) or missing from registry?");
                //address = "http://unknown";
            }

            options.AddUri(new Uri(new Uri(address), route));

            return new UriHealthCheck(options, () => serviceClient.HttpClient);
        }
    }
}

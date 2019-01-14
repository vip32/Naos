namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public abstract class DiscoveryProxy
    {
        public DiscoveryProxy(
            ILogger<DiscoveryProxy> logger,
            HttpClient httpClient,
            IDiscoveryClient discoveryClient,
            string serviceName = null,
            string serviceTag = null)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(discoveryClient, nameof(discoveryClient));

            var registrations = discoveryClient.ServicesAsync().Result;
            if (!serviceName.IsNullOrEmpty())
            {
                registrations = registrations?.Where(r => r.Name?.Equals(serviceName, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!serviceTag.IsNullOrEmpty())
            {
                registrations = registrations?.Where(r => serviceTag.EqualsAny(r.Tags));
            }

            var registration = registrations?.FirstOrDefault();
            if (registration != null)
            {
                httpClient.BaseAddress = new Uri($"{registration.Address}:{registration.Port}".TrimEnd(':'));
                logger.LogInformation("CLIENT discovery httpclient (service={ServiceName}, tag={ServiceTag}, address={BaseAddress})", serviceName, serviceTag, httpClient.BaseAddress);
            }
            else
            {
                logger.LogWarning("CLIENT discovery httpclient (name={ServiceName}, tag={ServiceTag}, address=not found in registry)", serviceName, serviceTag);
            }

            // TODO: get serviceregistration by name OR any of the tags

            //else
            //{
            //    throw new Exception("no active service registrations found");
            //}

            this.HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }
}

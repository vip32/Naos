namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public abstract class ServiceDiscoveryProxy // TODO: actual this is the 'client'
    {
        public ServiceDiscoveryProxy(
            ILogger<ServiceDiscoveryProxy> logger,
            HttpClient httpClient,
            IServiceDiscoveryClient discoveryClient,
            string serviceName = null,
            string serviceTag = null)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(discoveryClient, nameof(discoveryClient));

            if(serviceName.IsNullOrEmpty() && serviceTag.IsNullOrEmpty())
            {
                throw new ArgumentNullException("ServiceName and ServiceTag both cannot be null or empty");
            }

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
                logger.LogInformation($"{LogEventIdentifiers.ServiceDiscovery} proxy (service={{ServiceName}}, tag={serviceTag}, address={httpClient.BaseAddress})", serviceName);
            }
            else
            {
                logger.LogWarning($"{LogEventIdentifiers.ServiceDiscovery} proxy (name={{ServiceName}}, tag={serviceTag}, address=not found in registry)", serviceName);
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

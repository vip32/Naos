namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public abstract class ServiceDiscoveryClient // TODO: actual this is the 'client'
    {
        public ServiceDiscoveryClient(
            ILogger<ServiceDiscoveryClient> logger,
            HttpClient httpClient,
            IServiceRegistryClient registryClient,
            string serviceName = null,
            string serviceTag = null)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(registryClient, nameof(registryClient));

            if(serviceName.IsNullOrEmpty() && serviceTag.IsNullOrEmpty())
            {
                throw new ArgumentNullException("serviceName and serviceTag arguments cannot both be null or empty");
            }

            var registration = registryClient.ServicesAsync(serviceName, serviceTag).Result?.FirstOrDefault();
            if (registration != null)
            {
                httpClient.BaseAddress = new Uri($"{registration.Address}:{registration.Port}".TrimEnd(':'));

                // following are used by possible router to forward to correct instance
                //httpClient.DefaultRequestHeaders.Add("X-ServiceName", serviceName);
                //httpClient.DefaultRequestHeaders.Add("X-ServiceTag", serviceTag);

                logger.LogInformation($"{{LogKey}} proxy (service={{ServiceName}}, tag={serviceTag}, address={httpClient.BaseAddress})", LogEventKeys.ServiceDiscovery, serviceName);
            }
            else
            {
                logger.LogWarning($"{{LogKey}} proxy (name={{ServiceName}}, tag={serviceTag}, address=not found in registry)", LogEventKeys.ServiceDiscovery, serviceName);
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

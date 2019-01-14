namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public abstract class DiscoveryProxy
    {
        public DiscoveryProxy(ILogger<DiscoveryProxy> logger, HttpClient httpClient, IDiscoveryClient discoveryClient)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(discoveryClient, nameof(discoveryClient));

            var serviceRegistration = discoveryClient.ServicesAsync(this.ServiceName).Result.FirstOrDefault();
            if(serviceRegistration != null)
            {
                httpClient.BaseAddress = new Uri($"{serviceRegistration.Address}:{serviceRegistration.Port}".TrimEnd(':'));
                logger.LogInformation("CLIENT discovery httpclient (service={ServiceName}, address={BaseAddress})", this.ServiceName, httpClient.BaseAddress);
            }
            else
            {
                logger.LogWarning("CLIENT discovery httpclient (name={ServiceName}, address=not found in registry)", this.ServiceName);
            }

            // TODO: get serviceregistration by name OR any of the tags

            //else
            //{
            //    throw new Exception("no active service registrations found");
            //}

            this.HttpClient = httpClient;
        }

        public abstract string ServiceName { get; }

        public HttpClient HttpClient { get; }
    }
}

namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.ServiceDiscovery.App.Web;

    public abstract class ServiceDiscoveryRouterClient // TODO: actual this is the 'client'
    {
        public ServiceDiscoveryRouterClient(
            ILogger<ServiceDiscoveryRouterClient> logger,
            HttpClient httpClient,
            string serviceName = null,
            string serviceTag = null)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(logger, nameof(logger));

            if (serviceName.IsNullOrEmpty() && serviceTag.IsNullOrEmpty())
            {
                throw new ArgumentNullException("ServiceName and ServiceTag both cannot be null or empty");
            }

            // TODO: how to get router baseurl, inject/config?
            httpClient.BaseAddress = new Uri("https://router-endpoint");

            // following are used by router to forward to correct service instance
            httpClient.DefaultRequestHeaders.Add(ServiceDiscoveryRouterHeaders.ServiceName, serviceName);
            httpClient.DefaultRequestHeaders.Add(ServiceDiscoveryRouterHeaders.ServiceTag, serviceTag);

            logger.LogInformation($"{{LogKey}} router (service={{ServiceName}}, tag={serviceTag}, address={httpClient.BaseAddress})", LogEventKeys.ServiceDiscovery, serviceName);

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

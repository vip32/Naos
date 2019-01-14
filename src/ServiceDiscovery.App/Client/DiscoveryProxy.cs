namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;

    public abstract class DiscoveryProxy
    {
        public DiscoveryProxy(HttpClient httpClient, IDiscoveryClient discoveryClient)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(discoveryClient, nameof(discoveryClient));

            var serviceRegistration = discoveryClient.ServicesAsync(this.ServiceName).Result.FirstOrDefault();
            if(serviceRegistration != null)
            {
                httpClient.BaseAddress = new Uri($"{serviceRegistration.Address}:{serviceRegistration.Port}".TrimEnd(':'));
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

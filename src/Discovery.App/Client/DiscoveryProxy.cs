namespace Naos.Core.Discovery.App
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using EnsureThat;

    public abstract class DiscoveryProxy
    {
        //private readonly IDiscoveryClient discoveryClient;

        public DiscoveryProxy(HttpClient httpClient, IDiscoveryClient discoveryClient)
        {
            EnsureArg.IsNotNull(httpClient, nameof(httpClient)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2
            EnsureArg.IsNotNull(discoveryClient, nameof(discoveryClient));

            var serviceRegistration = discoveryClient.Services(this.ServiceName).FirstOrDefault();
            if(serviceRegistration != null)
            {
                httpClient.BaseAddress = new Uri($"{serviceRegistration.Address}:{serviceRegistration.Port}".TrimEnd(':'));
            }

            //else
            //{
            //    throw new Exception("no active service registration found");
            //}

            this.HttpClient = httpClient;
            //this.discoveryClient = discoveryClient;
        }

        public abstract string ServiceName { get; }

        public HttpClient HttpClient { get; }
    }

    //public class OrderServiceProxy : HttpClientDiscoveryProxy
    //{
    //}

    // services.AddSingleton<IServiceRegistry, ServiceRegistry>();
    // services.AddHttpClient<OrderServiceProxy>();

    // inject OrderServiceProxy where needed and use as OrderServiceProxy.Client for http requests

    // REGISTRATION (during app startup by using messaging? or just store in central db?)
    // https://cecilphillip.com/using-consul-for-service-discovery-with-asp-net-core/
    // Get server IP address
    // Register service with registry (type:AgentServiceRegistration)
}

namespace Naos.Core.Discovery.App
{
    using System;
    using System.Net.Http;
    using EnsureThat;

    public abstract class HttpClientDiscoveryProxy
    {
        private readonly IServiceRegistry registry;

        public HttpClientDiscoveryProxy(HttpClient client, IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(registry, nameof(registry)); // TYPED CLIENT > https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2

            client.BaseAddress = new Uri("https://api.github.com/"); // TODO: lookup base address (url) in local registry
            // GitHub API versioning
            // client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            // GitHub requires a user-agent
            // client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");

            this.Client = client;
            this.registry = registry;
        }

        public HttpClient Client { get; }
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

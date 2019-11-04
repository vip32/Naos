namespace Naos.Sample.Customers.Application.Client
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.ServiceDiscovery.Application;

    public class UserAccountsClient : ServiceDiscoveryClient
    {
        public UserAccountsClient(
            ILoggerFactory loggerFactory,
            ServiceDiscoveryConfiguration configuration,
            HttpClient httpClient,
            IServiceRegistryClient discoveryClient)
            : base(loggerFactory, configuration, httpClient, discoveryClient, "Product.Capability", "UserAccounts")
        {
        }
    }
}

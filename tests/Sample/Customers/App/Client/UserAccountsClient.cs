namespace Naos.Sample.Customers.App.Client
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.ServiceDiscovery.App;

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

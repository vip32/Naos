namespace Naos.Sample.Customers.App.Client
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;

    public class UserAccountsClient : ServiceDiscoveryClient
    {
        public UserAccountsClient(
            HttpClient httpClient,
            ServiceDiscoveryConfiguration configuration,
            ILogger<UserAccountsClient> logger,
            IServiceRegistryClient discoveryClient)
            : base(logger, configuration, httpClient, discoveryClient, "Product.Capability", "UserAccounts")
        {
        }
    }
}

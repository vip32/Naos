namespace Naos.Sample.Customers.App.Client
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;

    public class UserAccountsProxy : DiscoveryProxy
    {
        public UserAccountsProxy(HttpClient httpClient, ILogger<UserAccountsProxy> logger, IDiscoveryClient discoveryClient)
            : base(logger, httpClient, discoveryClient, "Product.Capability", "UserAccounts")
        {
        }
    }
}

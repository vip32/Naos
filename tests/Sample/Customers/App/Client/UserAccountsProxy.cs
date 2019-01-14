namespace Naos.Sample.Customers.App.Client
{
    using System.Net.Http;
    using Naos.Core.ServiceDiscovery.App;

    public class UserAccountsProxy : DiscoveryProxy
    {
        public UserAccountsProxy(HttpClient httpClient, IDiscoveryClient discoveryClient)
            : base(httpClient, discoveryClient)
        {
        }

        public override string ServiceName => "Sample.UserAccounts"; //= ServiceDescriptor.Product ServiceDescriptor.Capability
    }
}

namespace Naos.Core.ServiceDiscovery.App.Web.Router
{
    public class RouterContext
    {
        public RouterContext(IServiceRegistryClient registryClient)
        {
            this.RegistryClient = registryClient;
        }

        public IServiceRegistryClient RegistryClient { get; }
    }
}

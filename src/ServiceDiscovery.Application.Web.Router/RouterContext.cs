namespace Naos.ServiceDiscovery.Application.Web.Router
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

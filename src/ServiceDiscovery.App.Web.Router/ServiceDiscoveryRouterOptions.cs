namespace Naos.Core.ServiceDiscovery.App.Web.Router
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceDiscoveryRouterOptions
    {
        public ServiceDiscoveryRouterOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}

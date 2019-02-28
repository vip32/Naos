namespace Naos.Core.ServiceDiscovery.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceDiscoveryOptions
    {
        public ServiceDiscoveryOptions(INaosServicesContext context)
        {
            this.Context = context;
        }

        public INaosServicesContext Context { get; }
    }
}

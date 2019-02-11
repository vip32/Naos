namespace Naos.Core.ServiceDiscovery.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceDiscoveryOptions
    {
        public ServiceDiscoveryOptions(ServiceConfigurationContext context)
        {
            this.Context = context;
        }

        public ServiceConfigurationContext Context { get; }
    }
}

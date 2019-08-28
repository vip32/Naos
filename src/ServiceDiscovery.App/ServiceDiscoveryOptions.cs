namespace Naos.Core.ServiceDiscovery.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceDiscoveryOptions
    {
        public ServiceDiscoveryOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}

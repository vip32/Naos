namespace Naos.Core.ServiceDiscovery.App
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceDiscoveryOptions
    {
        public ServiceDiscoveryOptions(INaosBuilder context)
        {
            this.Context = context;
        }

        public INaosBuilder Context { get; }
    }
}

namespace Naos.ServiceDiscovery.Application.Web.Router
{
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceDiscoveryRouterOptions
    {
        public ServiceDiscoveryRouterOptions(INaosBuilderContext context)
        {
            this.Context = context;
        }

        public INaosBuilderContext Context { get; }
    }
}

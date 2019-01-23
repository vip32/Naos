namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;

    public class ServiceRegistryClient : IServiceRegistryClient
    {
        private readonly IServiceRegistry registry;

        public ServiceRegistryClient(IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(registry, nameof(registry));

            this.registry = registry;
        }

        public async Task<IEnumerable<ServiceRegistration>> ServicesAsync()
        {
            return (await this.registry.RegistrationsAsync()).Safe();
        }

        public async Task<IEnumerable<ServiceRegistration>> ServicesAsync(string name)
        {
            return (await this.registry.RegistrationsAsync()).Safe()
                .Where(r => r.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}

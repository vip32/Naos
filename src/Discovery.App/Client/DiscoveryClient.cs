namespace Naos.Core.Discovery.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Naos.Core.Common;

    public class DiscoveryClient : IDiscoveryClient
    {
        private readonly IServiceRegistry registry;

        public DiscoveryClient(IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(registry, nameof(registry));

            this.registry = registry;
        }

        public IEnumerable<ServiceRegistration> Services()
        {
            return this.registry.Registrations().NullToEmpty();
        }

        public IEnumerable<ServiceRegistration> Services(string name)
        {
            return this.registry.Registrations().NullToEmpty().Where(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}

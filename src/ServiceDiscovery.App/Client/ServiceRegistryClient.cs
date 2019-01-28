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
        private readonly IServiceRegistry registryClient;

        public ServiceRegistryClient(IServiceRegistry registryClient)
        {
            EnsureArg.IsNotNull(registryClient, nameof(registryClient));

            this.registryClient = registryClient;
        }

        public async Task<IEnumerable<ServiceRegistration>> ServicesAsync()
        {
            return (await this.registryClient.RegistrationsAsync()).Safe();
        }

        public async Task<IEnumerable<ServiceRegistration>> ServicesAsync(string name, string tag)
        {
            var registrations = await this.registryClient.RegistrationsAsync().ConfigureAwait(false);

            if (!name.IsNullOrEmpty())
            {
                registrations = registrations?.Where(r => r.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!tag.IsNullOrEmpty())
            {
                registrations = registrations?.Where(r => tag.EqualsAny(r.Tags));
            }

            return registrations;
        }
    }
}

namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;

    public class ServiceRegistryClient : IServiceRegistryClient // acts as a facade for IServiceRegistry
    {
        private readonly IServiceRegistry registry;

        public ServiceRegistryClient(IServiceRegistry registry)
        {
            EnsureArg.IsNotNull(registry, nameof(registry));

            this.registry = registry;
        }

        public async Task DeRegisterAsync(string id)
        {
            EnsureArg.IsNotNullOrEmpty(id, nameof(id));

            await this.registry.DeRegisterAsync(id).AnyContext();
        }

        public async Task RegisterAsync(ServiceRegistration registration)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));

            await this.registry.RegisterAsync(registration).AnyContext();
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            return (await this.registry.RegistrationsAsync()).Safe();
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync(string name, string tag)
        {
            var registrations = await this.registry.RegistrationsAsync().AnyContext();

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

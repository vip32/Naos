namespace Naos.Core.ServiceDiscovery.App
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class FileSystemServiceRegistry : IServiceRegistry
    {
        private readonly ILogger<FileSystemServiceRegistry> logger;
        private readonly FileSystemServiceRegistryConfiguration configuration;
        private List<ServiceRegistration> store = new List<ServiceRegistration>();

        public FileSystemServiceRegistry(ILogger<FileSystemServiceRegistry> logger, FileSystemServiceRegistryConfiguration configuration)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.configuration = configuration;

            // TODO: inject HealthStrategy which can validate the registrations
        }

        public Task DeRegisterAsync(string id)
        {
            // TODO: remove registration
            return Task.CompletedTask;
        }

        public Task RegisterAsync(ServiceRegistration registration)
        {
            // TODO: store registration
            this.store.Add(registration);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            //return Enumerable.Empty<ServiceRegistration>(); // omit unhealthy registrations!
            return await Task.Run(() => this.store).ConfigureAwait(false);
        }
    }
}

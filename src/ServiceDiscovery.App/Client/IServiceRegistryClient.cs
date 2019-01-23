namespace Naos.Core.ServiceDiscovery.App
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServiceRegistryClient
    {
        Task<IEnumerable<ServiceRegistration>> ServicesAsync();

        Task<IEnumerable<ServiceRegistration>> ServicesAsync(string name);
    }
}
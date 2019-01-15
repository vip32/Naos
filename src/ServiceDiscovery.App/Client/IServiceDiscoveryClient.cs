namespace Naos.Core.ServiceDiscovery.App
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServiceDiscoveryClient
    {
        Task<IEnumerable<ServiceRegistration>> ServicesAsync();

        Task<IEnumerable<ServiceRegistration>> ServicesAsync(string name);
    }
}
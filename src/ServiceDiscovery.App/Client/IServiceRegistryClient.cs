namespace Naos.ServiceDiscovery.App
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServiceRegistryClient
    {
        Task RegisterAsync(ServiceRegistration registration);

        Task DeRegisterAsync(string id);

        Task<IEnumerable<ServiceRegistration>> RegistrationsAsync();

        Task<IEnumerable<ServiceRegistration>> RegistrationsAsync(string name, string tag);
    }
}
namespace Naos.ServiceDiscovery.Application
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServiceRegistry
    {
        Task RegisterAsync(ServiceRegistration registration);

        Task DeRegisterAsync(string id);

        Task<IEnumerable<ServiceRegistration>> RegistrationsAsync();
    }
}

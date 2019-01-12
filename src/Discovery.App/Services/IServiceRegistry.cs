namespace Naos.Core.Discovery.App
{
    using System.Collections.Generic;

    public interface IServiceRegistry
    {
        void Register(ServiceRegistration registration);

        void DeRegister(string id);

        IEnumerable<ServiceRegistration> Registrations();
    }
}

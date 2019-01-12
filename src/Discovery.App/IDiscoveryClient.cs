namespace Naos.Core.Discovery.App
{
    using System.Collections.Generic;

    public interface IDiscoveryClient
    {
        IEnumerable<ServiceRegistration> Services();

        IEnumerable<ServiceRegistration> Services(string name);
    }
}
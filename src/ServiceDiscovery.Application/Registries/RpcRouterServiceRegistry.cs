namespace Naos.ServiceDiscovery.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RpcRouterServiceRegistry : IServiceRegistry
    {
        public Task DeRegisterAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task RegisterAsync(ServiceRegistration registration)
        {
            // TODO: call remote service https://github.com/damienbod/Secure_gRpc/blob/master/Secure_gRpc/Secure_gRpc.Client/Program.cs
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

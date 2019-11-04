namespace Naos.ServiceDiscovery.Application.Web.Router
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RpcRouterService
    {
        private readonly IServiceRegistry registry;

        public RpcRouterService(IServiceRegistry registry)
        {
            this.registry = registry;
        }

        // TODO: implement service https://github.com/damienbod/Secure_gRpc/blob/master/Secure_gRpc/Secure_gRpc.Server/Services/GreeterService.cs
        //       service has registry injected where registrations are stored
        public Task DeRegisterAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task RegisterAsync(ServiceRegistration registration)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

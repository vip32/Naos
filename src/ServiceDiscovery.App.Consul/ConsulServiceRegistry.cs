namespace Naos.Core.ServiceDiscovery.App.Consul
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using global::Consul;
    using Microsoft.Extensions.Logging;

    public class ConsulServiceRegistry : IServiceRegistry
    {
        private readonly ILogger<ConsulServiceRegistry> logger;
        private readonly IConsulClient client;

        public ConsulServiceRegistry(ILogger<ConsulServiceRegistry> logger, IConsulClient client)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(client, nameof(client));

            this.logger = logger;
            this.client = client;
        }

        public async Task DeRegisterAsync(string id)
        {
            try
            {
                await this.client.Agent.ServiceDeregister(id);
            }
            catch
            {
                this.logger.LogError($"consul failed deregistration {id}");
            }
        }

        public async Task RegisterAsync(ServiceRegistration registration)
        {
            var agentRegistration = new AgentServiceRegistration()
            {
                ID = registration.Id,
                Name = registration.Name,
                Address = registration.Address,
                Port = registration.Port,
                Tags = registration.Tags
            };

            await this.client.Agent.ServiceDeregister(agentRegistration.ID);
            this.logger.LogError($"consul registered {registration.Id}");
            await this.client.Agent.ServiceRegister(agentRegistration);
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            var services = await this.client.Agent.Services().ConfigureAwait(false);

            return services.Response.Values.Select(s => new ServiceRegistration
            {
                Id = s.ID,
                // Name = s., not available?
                Address = s.Address,
                Port = s.Port,
                Tags = s.Tags
            });
        }
    }
}

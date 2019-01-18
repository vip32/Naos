namespace Naos.Core.ServiceDiscovery.App.Consul
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using global::Consul;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

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

            this.logger.LogInformation($"{LogEventIdentifiers.ServiceDiscovery} consul active");
        }

        public async Task DeRegisterAsync(string id)
        {
            try
            {
                this.logger.LogInformation($"{LogEventIdentifiers.ServiceDiscovery} consul registration delete (id={{RegistrationId}})", id);
                await this.client.Agent.ServiceDeregister(id);
            }
            catch
            {
                this.logger.LogError($"{LogEventIdentifiers.ServiceDiscovery} consul deregister failed {id}");
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
                Tags = registration.Tags,
                Check = registration.Check == null ? null : new AgentServiceCheck
                {
                    HTTP = $"{registration.Address}:{registration.Port}".TrimEnd(':') + $"/{registration.Check.Route}".Replace("//", "/"),
                    Interval = registration.Check.Interval
                }
            };

            this.logger.LogInformation($"{LogEventIdentifiers.ServiceDiscovery} consul register (name={{RegistrationName}}, tags={string.Join("|", registration.Tags.NullToEmpty())}, id={{RegistrationId}})", registration.Name, registration.Id);
            await this.client.Agent.ServiceDeregister(agentRegistration.ID);
            await this.client.Agent.ServiceRegister(agentRegistration);
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            var services = await this.client.Agent.Services().ConfigureAwait(false);

            return services.Response.Values.Select(s => new ServiceRegistration
            {
                Id = s.ID,
                Name = s.Service,
                Address = s.Address,
                Port = s.Port,
                Tags = s.Tags
            });
        }
    }
}

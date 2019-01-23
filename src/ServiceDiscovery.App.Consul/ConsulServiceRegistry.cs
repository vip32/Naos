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

            this.logger.LogInformation("{LogKey} consul active", LogEventKeys.ServiceDiscovery);
        }

        public async Task DeRegisterAsync(string id)
        {
            try
            {
                this.logger.LogInformation("{LogKey} consul registration delete (id={RegistrationId})", LogEventKeys.ServiceDiscovery, id);
                await this.client.Agent.ServiceDeregister(id);
            }
            catch
            {
                this.logger.LogError($"{{LogKey}} consul deregister failed {id}", LogEventKeys.ServiceDiscovery);
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
                    HTTP = registration.FullAddress + $"/{registration.Check.Route}".Replace("//", "/"),
                    Interval = registration.Check.Interval
                }
            };

            this.logger.LogInformation($"{{LogKey}} register consul (name={{RegistrationName}}, tags={string.Join("|", registration.Tags.Safe())}, id={{RegistrationId}}, address={registration.FullAddress})", LogEventKeys.ServiceDiscovery, registration.Name, registration.Id);
            await this.client.Agent.ServiceDeregister(agentRegistration.ID).ConfigureAwait(false);
            var result = await this.client.Agent.ServiceRegister(agentRegistration).ConfigureAwait(false);
            this.logger.LogInformation($"{{LogKey}} register consul {result.StatusCode} (name={{RegistrationName}}, id={{RegistrationId}})", LogEventKeys.ServiceDiscovery, registration.Name, registration.Id);
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

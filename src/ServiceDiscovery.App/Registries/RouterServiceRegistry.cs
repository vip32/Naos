namespace Naos.ServiceDiscovery.App
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;

    public class RouterServiceRegistry : IServiceRegistry
    {
        private readonly ILogger<RouterServiceRegistry> logger;
        private readonly HttpClient httpClient;

        public RouterServiceRegistry(
            ILogger<RouterServiceRegistry> logger,
            HttpClient httpClient,
            RouterServiceRegistryConfiguration configuration)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(httpClient, nameof(httpClient));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNullOrEmpty(configuration.Address, nameof(configuration.Address));

            this.logger = logger;
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(configuration.Address);
        }

        public async Task DeRegisterAsync(string id)
        {
            EnsureArg.IsNotNullOrEmpty(id, nameof(id));

            await this.httpClient.DeleteAsync($"api/servicediscovery/router/registrations/{id}").AnyContext();
        }

        public async Task RegisterAsync(ServiceRegistration registration)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));

            using (var content = new JsonContent(registration))
            {
                await this.httpClient.PostAsync("api/servicediscovery/router/registrations", content).AnyContext();
            }
        }

        public Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

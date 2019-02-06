namespace Naos.Core.ServiceDiscovery.App
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Web.Client;

    public class RemoteServiceRegistry : IServiceRegistry
    {
        private readonly ILogger<RemoteServiceRegistry> logger;
        private readonly HttpClient httpClient;

        public RemoteServiceRegistry(
            ILogger<RemoteServiceRegistry> logger,
            HttpClient httpClient,
            RemoteServiceRegistryConfiguration configuration)
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

            await this.httpClient.DeleteAsync($"api/router/registrations/{id}").AnyContext();
        }

        public async Task RegisterAsync(ServiceRegistration registration)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));

            await this.httpClient.PostAsync("api/router/registrations", new JsonContent(registration)).AnyContext();
        }

        public Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}

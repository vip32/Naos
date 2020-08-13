namespace Naos.ServiceDiscovery.Application.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class ServiceDiscoveryHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<ServiceDiscoveryHostedService> logger;
        private readonly ServiceDiscoveryConfiguration configuration;
        private readonly IServiceRegistryClient registryClient;
        private readonly IServer server;
        private readonly ServiceDescriptor serviceDescriptor;
        private string serviceAddress;
        private CancellationTokenSource cts;
        private bool registered;
        private List<string> registrationIds = new List<string>();

        public ServiceDiscoveryHostedService(
            ILogger<ServiceDiscoveryHostedService> logger,
            ServiceDiscoveryConfiguration configuration,
            IServiceRegistryClient registryClient,
            IServer server,
            ServiceDescriptor serviceDescriptor)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNull(registryClient, nameof(registryClient));
            EnsureArg.IsNotNull(server, nameof(server));
            EnsureArg.IsNotNull(serviceDescriptor, nameof(serviceDescriptor));

            this.logger = logger;
            this.configuration = configuration;
            this.registryClient = registryClient;
            this.server = server;
            this.serviceDescriptor = serviceDescriptor;
            this.serviceAddress = this.configuration.ServiceAddresses?.FirstOrDefault(); // TODO: register all addresses (foreach)
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service started", LogKeys.ServiceDiscovery);

            this.cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // https://github.com/cecilphillip/aspnet-servicediscovery-patterns/blob/master/self_registration/src/SchoolAPI/Infrastructure/ConsulHostedService.cs

            if (this.serviceAddress.IsNullOrEmpty())
            {
                var features = this.server.Features;
                var addressFeature = features?.Get<IServerAddressesFeature>();
                this.serviceAddress = addressFeature?.Addresses?.FirstOrDefault(); // TODO: register all addresses (foreach)
            }

            if (!this.serviceAddress.IsNullOrEmpty())
            {
                // Register this service (use ServiceDescriptor for more infos)
                var uri = new Uri(this.serviceAddress);
                var registrationId = $"{this.serviceDescriptor.Name}-{HashAlgorithm.ComputeMd5Hash(uri.ToString())}";
                this.logger.LogInformation($"{{LogKey:l}} service registration (id={registrationId}, address={this.serviceAddress})", LogKeys.ServiceDiscovery);

                var registration = new ServiceRegistration
                {
                    Id = registrationId, // TODO: use resolved servicedescriptor for id/name (AppDomain.CurrentDomain.FriendlyName)
                    Name = this.serviceDescriptor.Name,
                    Address = $"{uri.Scheme}://{uri.Host}",
                    Port = uri.Port,
                    Tags = this.serviceDescriptor.Tags
                };

                //this.logger.LogInformation($"{LogEventIdentifiers.ServiceDiscovery} register (name={{RegistrationName}}, address={registration.FullAddress})", registration.Name);
                this.registryClient.RegisterAsync(registration);
                this.registrationIds.Add(registrationId);
                this.registered = true;
            }
            else
            {
                this.logger.LogWarning("{LogKey:l} service registration failed, missing address", LogKeys.ServiceDiscovery);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service stopped", LogKeys.ServiceDiscovery);

            this.cts?.Cancel();

            if (this.registered)
            {
                foreach (var registrationId in this.registrationIds.Safe())
                {
                    this.registryClient.DeRegisterAsync(registrationId);
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.cts?.Dispose();
        }
    }
}

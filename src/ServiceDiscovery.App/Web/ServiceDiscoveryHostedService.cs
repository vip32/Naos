﻿namespace Naos.Core.ServiceDiscovery.App.Web
{
    using System;
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
        private string registrationId;
        private bool registered = false;

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
            this.serviceAddress = this.configuration.ServiceAddresses?.FirstOrDefault();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service started", LogKeys.ServiceDiscovery);

            this.cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // https://github.com/cecilphillip/aspnet-servicediscovery-patterns/blob/master/self_registration/src/SchoolAPI/Infrastructure/ConsulHostedService.cs
            if(this.serviceAddress.IsNullOrEmpty())
            {
                this.logger.LogInformation("{LogKey:l} 1", LogKeys.ServiceDiscovery);

                var features = this.server.Features;
                this.logger.LogInformation($"{{LogKey:l}} 2 (features={features != null})", LogKeys.ServiceDiscovery);

                var addressFeature = features?.Get<IServerAddressesFeature>();
                this.logger.LogInformation($"{{LogKey:l}} 3 (addressFeature={addressFeature != null})", LogKeys.ServiceDiscovery);

                this.serviceAddress = addressFeature?.Addresses?.FirstOrDefault();
                this.logger.LogInformation("{LogKey:l} 4", LogKeys.ServiceDiscovery);
            }

            if(this.serviceAddress != null)
            {
                this.logger.LogInformation($"{{LogKey:l}} service registration (address={this.serviceAddress})", LogKeys.ServiceDiscovery);

                // Register this service (use ServiceDescriptor for more infos)
                var uri = new Uri(this.serviceAddress);
                this.registrationId = $"{this.serviceDescriptor.Name}-{HashAlgorithm.ComputeMd5Hash(uri.ToString())}";
                var registration = new ServiceRegistration
                {
                    Id = this.registrationId, // TODO: use resolved servicedescriptor for id/name (AppDomain.CurrentDomain.FriendlyName)
                    Name = this.serviceDescriptor.Name,
                    Address = $"{uri.Scheme}://{uri.Host}",
                    Port = uri.Port,
                    Tags = this.serviceDescriptor.Tags
                };

                //this.logger.LogInformation($"{LogEventIdentifiers.ServiceDiscovery} register (name={{RegistrationName}}, address={registration.FullAddress})", registration.Name);
                this.registryClient.RegisterAsync(registration);
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

            this.cts.Cancel();

            if(this.registered)
            {
                this.registryClient.DeRegisterAsync(this.registrationId);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}

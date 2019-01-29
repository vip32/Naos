namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Consul;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Consul;
    using Naos.Core.ServiceDiscovery.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddServiceDiscoveryClientConsul(
            this ServiceConfigurationContext context,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.AddSingleton(sp => context.Configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            context.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(c =>
            {
                c.Address = new Uri(context.Configuration.GetSection($"{section}:registry:consul").Get<ConsulServiceRegistryConfiguration>().Address);
            }));
            context.Services.TryAddSingleton<IServiceRegistry>(sp => new ConsulServiceRegistry(
                sp.GetRequiredService<ILogger<ConsulServiceRegistry>>(), sp.GetRequiredService<IConsulClient>()));
            context.Services.TryAddSingleton<IServiceRegistryClient, ServiceRegistryClient>();

            return context;
        }
    }
}

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
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosDiscoveryConsul(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.AddSingleton(sp => configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(c =>
            {
                c.Address = new Uri(configuration.GetSection($"{section}:registry:consul").Get<ConsulServiceRegistryConfiguration>().Address);
            }));
            services.TryAddSingleton<IServiceRegistry>(sp => new ConsulServiceRegistry(
                sp.GetRequiredService<ILogger<ConsulServiceRegistry>>(), sp.GetRequiredService<IConsulClient>()));
            services.TryAddSingleton<IServiceDiscoveryClient, ServiceDiscoveryClient>();

            return services;
        }
    }
}

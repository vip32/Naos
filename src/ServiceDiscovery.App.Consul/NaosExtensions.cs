namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Consul;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.App;
    using Naos.Foundation;
    using Naos.ServiceDiscovery.App;
    using Naos.ServiceDiscovery.App.Consul;
    using Naos.ServiceDiscovery.App.Web;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        public static ServiceDiscoveryOptions UseConsulClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(c =>
            {
                c.Address = new Uri(options.Context.Configuration?.GetSection($"{section}:registry:consul").Get<ConsulServiceRegistryConfiguration>().Address);
            }));
            options.Context.Services.TryAddSingleton<IServiceRegistry>(sp => new ConsulServiceRegistry(
                sp.GetRequiredService<ILogger<ConsulServiceRegistry>>(), sp.GetRequiredService<IConsulClient>()));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: service discovery added (type={nameof(ConsulServiceRegistry)})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscovery", Description = "Consul" });

            return options;
        }
    }
}

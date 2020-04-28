namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.ServiceDiscovery.Application;
    using Naos.ServiceDiscovery.Application.Web;
    //using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddServiceDiscovery(
            this NaosServicesContextOptions naosOptions,
            Action<ServiceDiscoveryOptions> optionsAction = null,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            var registryConfiguration = naosOptions.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>();
            naosOptions.Context.Services.AddSingleton(registryConfiguration);
            //naosOptions.Context.Services.AddSingleton(sp =>
            //    naosOptions.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>());

            naosOptions.Context.Services.AddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            optionsAction?.Invoke(new ServiceDiscoveryOptions(naosOptions.Context));

            naosOptions.Context.Messages.Add($"naos services builder: service discovery added (enabled={registryConfiguration.Enabled})");

            return naosOptions;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        public static ServiceDiscoveryOptions UseFileSystemClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var registryConfiguration = options.Context.Configuration?.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>();
            if (registryConfiguration.Folder.IsNullOrEmpty())
            {
                registryConfiguration.Folder = Path.Combine(Path.GetTempPath(), "naos_servicediscovery");
            }

            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new FileSystemServiceRegistry(
                    sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                    registryConfiguration));

            options.Context.Messages.Add($"naos services builder: service discovery registry added (type={nameof(FileSystemServiceRegistry)}, folder={registryConfiguration.Folder})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscovery", Description = "FileSystemClientRegistry", EchoRoute = "naos/servicediscovery/echo" });

            return options;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        public static ServiceDiscoveryOptions UseRouterClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            // client needs remote registry
            var registryConfiguration = options.Context.Configuration?.GetSection($"{section}:registry:router").Get<RouterServiceRegistryConfiguration>();
            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new RouterServiceRegistry(
                    sp.GetRequiredService<ILogger<RouterServiceRegistry>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    registryConfiguration));

            // overwrite ServiceDiscoveryConfiguration (from AddServiceDiscovery) with router infos
            options.Context.Services.AddSingleton(sp =>
                {
                    var configuration = options.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>();
                    configuration.RouterAddress = registryConfiguration.Address;
                    configuration.RouterPath = "naos/servicediscovery/router/proxy";
                    configuration.RouterEnabled = true;
                    return configuration;
                });

            options.Context.Messages.Add($"naos services builder: service discovery registry added (type={nameof(RouterServiceRegistry)})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscovery", Description = "RouterClientRegistry", EchoRoute = "naos/servicediscovery/echo" });

            return options;
        }
    }
}

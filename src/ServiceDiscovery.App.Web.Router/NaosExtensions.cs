namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web.Router;
    using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddServiceDiscoveryRouter(
            this NaosServicesContextOptions naosOptions,
            Action<ServiceDiscoveryRouterOptions> optionsAction = null,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.AddProxy(o =>
            {
                //o.ConfigurePrimaryHttpMessageHandler(c => c.GetRequiredService<HttpClientLogHandler>());
                //o.AddHttpMessageHandler<HttpClientLogHandler>();
            });

            optionsAction?.Invoke(new ServiceDiscoveryRouterOptions(naosOptions.Context));

            return naosOptions;
        }

        /// <summary>
        /// Adds required services to support the Discovery router functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        public static ServiceDiscoveryRouterOptions UseFileSystemRegistry(
            this ServiceDiscoveryRouterOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            var registryConfiguration = options.Context.Configuration?.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>();
            if(registryConfiguration.Folder.IsNullOrEmpty())
            {
                registryConfiguration.Folder = Path.Combine(Path.Combine(Path.GetTempPath(), "naos_servicediscovery"), "router");
            }

            options.Context.Services.AddSingleton(sp =>
                new RouterContext(
                    new ServiceRegistryClient(
                        new FileSystemServiceRegistry(
                            sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                            registryConfiguration))));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: service discovery router added (registry={nameof(FileSystemServiceRegistry)})");
            options.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "ServiceDiscoveryRouter", EchoRoute = "api/echo/servicediscovery/router" });

            return options;
        }
    }
}

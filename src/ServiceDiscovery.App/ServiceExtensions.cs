namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web;
    //using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        public static NaosOptions AddServiceDiscovery(
            this NaosOptions naosOptions,
            Action<ServiceDiscoveryOptions> setupAction = null,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            naosOptions.Context.Services.AddSingleton(sp => naosOptions.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            naosOptions.Context.Services.AddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            setupAction?.Invoke(new ServiceDiscoveryOptions(naosOptions.Context));

            //context.Messages.Add($"{LogEventKeys.General} naos builder: service discovery added");

            return naosOptions;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions UseFileSystemClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new FileSystemServiceRegistry(
                    sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                    options.Context.Configuration?.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()));

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: service discovery added (registry={nameof(FileSystemServiceRegistry)})");

            return options;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions UseRemoteRouterClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            // client needs remote registry
            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new RemoteServiceRegistry(
                    sp.GetRequiredService<ILogger<RemoteServiceRegistry>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    options.Context.Configuration?.GetSection($"{section}:registry:remote").Get<RemoteServiceRegistryConfiguration>()));

            options.Context.Messages.Add($"{LogEventKeys.Startup} naos builder: service discovery added (registry={nameof(RemoteServiceRegistry)})");

            return options;
        }
    }
}

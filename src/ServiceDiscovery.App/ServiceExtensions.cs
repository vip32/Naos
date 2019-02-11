namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;
    using Naos.Core.ServiceDiscovery.App.Web;
    //using ProxyKit;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions
    {
        public static ServiceConfigurationContext AddServiceDiscovery(
            this ServiceConfigurationContext context,
            Action<ServiceDiscoveryOptions> setupAction = null)
        {
            setupAction?.Invoke(new ServiceDiscoveryOptions(context));

            return context;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions AddFileSystemClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.Services.AddSingleton(sp => options.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new FileSystemServiceRegistry(
                    sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                    options.Context.Configuration?.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()));
            options.Context.Services.AddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            return options;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceDiscoveryOptions AddRemoteRouterClientRegistry(
            this ServiceDiscoveryOptions options,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            // client needs remote registry
            options.Context.Services.AddSingleton(sp => options.Context.Configuration?.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            options.Context.Services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            options.Context.Services.AddSingleton<IServiceRegistry>(sp =>
                new RemoteServiceRegistry(
                    sp.GetRequiredService<ILogger<RemoteServiceRegistry>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    options.Context.Configuration?.GetSection($"{section}:registry:remote").Get<RemoteServiceRegistryConfiguration>()));
            options.Context.Services.AddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            return options;
        }
    }
}

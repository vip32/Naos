namespace Microsoft.Extensions.DependencyInjection
{
    using System.Net.Http;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddServiceDiscoveryClientFileSystem(
            this ServiceConfigurationContext context,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            context.Services.TryAddSingleton(sp => context.Configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            context.Services.TryAddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            context.Services.TryAddSingleton<IServiceRegistry>(sp =>
                new FileSystemServiceRegistry(
                    sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                    context.Configuration.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()));
            context.Services.TryAddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            return context;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddServiceDiscoveryClientRemote(
            this ServiceConfigurationContext context,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            //services.AddProxy(o =>
            //{
            //    //o.ConfigurePrimaryHttpMessageHandler(c => c.GetRequiredService<HttpClientLogHandler>());
            //    //o.AddHttpMessageHandler<HttpClientLogHandler>();
            //});

            // client needs remote registry
            context.Services.TryAddSingleton(sp => context.Configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            context.Services.TryAddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            context.Services.TryAddSingleton<IServiceRegistry>(sp =>
                new RemoteServiceRegistry(
                    sp.GetRequiredService<ILogger<RemoteServiceRegistry>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    context.Configuration.GetSection($"{section}:registry:remote").Get<RemoteServiceRegistryConfiguration>()));
            context.Services.TryAddSingleton<IServiceRegistryClient>(sp =>
                new ServiceRegistryClient(sp.GetRequiredService<IServiceRegistry>()));

            // router service needs different registry!

            return context;
        }
    }
}

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
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosServiceDiscoveryFileSystem(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.TryAddSingleton(sp => configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            services.TryAddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            services.TryAddSingleton<IServiceRegistry>(sp =>
                new FileSystemServiceRegistry(
                    sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(),
                    configuration.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()));
            services.TryAddSingleton<IServiceRegistryClient, ServiceRegistryClient>();

            return services;
        }

        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosServiceDiscoveryRemote(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            //services.AddProxy(o =>
            //{
            //    //o.ConfigurePrimaryHttpMessageHandler(c => c.GetRequiredService<HttpClientLogHandler>());
            //    //o.AddHttpMessageHandler<HttpClientLogHandler>();
            //});

            // client needs remote registry
            services.TryAddSingleton(sp => configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            services.TryAddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            services.TryAddSingleton<IServiceRegistry>(sp =>
                new RemoteServiceRegistry(
                    sp.GetRequiredService<ILogger<RemoteServiceRegistry>>(),
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    configuration.GetSection($"{section}:registry:remote").Get<RemoteServiceRegistryConfiguration>()));
            services.TryAddSingleton<IServiceRegistryClient, ServiceRegistryClient>();

            // router service needs different registry!

            return services;
        }
    }
}

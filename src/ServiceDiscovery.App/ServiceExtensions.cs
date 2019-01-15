namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.ServiceDiscovery.App;
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
        public static IServiceCollection AddNaosServiceDiscoveryFileSystem(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:serviceDiscovery")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.AddSingleton(sp => configuration.GetSection(section).Get<ServiceDiscoveryConfiguration>());
            services.AddSingleton<IHostedService, ServiceDiscoveryHostedService>();
            services.TryAddSingleton<IServiceRegistry>(sp => new FileSystemServiceRegistry(
                sp.GetRequiredService<ILogger<FileSystemServiceRegistry>>(), configuration.GetSection($"{section}:registry:fileSystem").Get<FileSystemServiceRegistryConfiguration>()));
            services.TryAddSingleton<IServiceDiscoveryClient, ServiceDiscoveryClient>();

            return services;
        }
    }
}

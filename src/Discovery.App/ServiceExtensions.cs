namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Discovery.App;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceExtensions // TODO: rename to ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosDiscoveryFileSystem(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:serviceDiscovery:registries:fileSystem")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var fileSystemConfiguration = configuration.GetSection(section).Get<FileSystemServiceRegistryConfiguration>();

            services.TryAddSingleton<IServiceRegistry>(sp => new FileSystemServiceRegistry(fileSystemConfiguration));

            return services;
        }
    }
}

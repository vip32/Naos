namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Naos.Core.Discovery.App;

    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceRegistrations // TODO: rename to ServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNaosDiscoveryLocal(this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.TryAddSingleton<IServiceRegistry, LocalServiceRegistry>();

            return services;
        }
    }
}

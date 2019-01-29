namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.App;

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
        /// <param name="product"></param>
        /// <param name="capability"></param>
        /// <param name="tags"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ServiceConfigurationContext AddNaos(
            this IServiceCollection services,
            IConfiguration configuration,
            string product = null,
            string capability = null,
            string[] tags = null,
            string section = "naos")
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            var naosConfiguration = configuration.GetSection(section).Get<NaosConfiguration>();
            return new ServiceConfigurationContext
            {
                Services = services,
                Configuration = configuration,
                Descriptor = new Naos.Core.Commands.ServiceDescriptor(
                    product ?? naosConfiguration.Product,
                    capability ?? naosConfiguration.Product,
                    tags ?? naosConfiguration.Tags),
            };
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Configuration.App;

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
        /// <param name="setupAction"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static INaosBuilder AddNaos(
            this IServiceCollection services,
            IConfiguration configuration,
            string product = null,
            string capability = null,
            string[] tags = null,
            Action<ServiceOptions> setupAction = null,
            string section = "naos")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var naosConfiguration = configuration?.GetSection(section).Get<NaosConfiguration>();
            var context = new NaosBuilder
            {
                Services = services,
                Configuration = configuration,
                Descriptor = new Naos.Core.Common.ServiceDescriptor(
                    product ?? naosConfiguration.Product,
                    capability ?? naosConfiguration.Product,
                    tags: tags ?? naosConfiguration.Tags),
            };
            setupAction?.Invoke(new ServiceOptions(context));
            return context;
        }

        public static INaosBuilder AddServices(
            this INaosBuilder context,
            Action<ServiceOptions> setupAction = null)
        {
            setupAction?.Invoke(new ServiceOptions(context));

            return context;
        }
    }
}

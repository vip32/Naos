namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
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
        public static INaosBuilderContext AddNaos(
            this IServiceCollection services,
            IConfiguration configuration,
            string product = null,
            string capability = null,
            string[] tags = null,
            Action<NaosOptions> setupAction = null,
            string section = "naos")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var naosConfiguration = configuration?.GetSection(section).Get<NaosConfiguration>();
            var context = new NaosBuilderContext
            {
                Services = services,
                Configuration = configuration,
                Descriptor = new Naos.Core.Common.ServiceDescriptor(
                    product ?? naosConfiguration.Product,
                    capability ?? naosConfiguration.Product,
                    tags: tags ?? naosConfiguration.Tags),
            };
            setupAction?.Invoke(new NaosOptions(context));

            var logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("Naos");
            foreach (var message in context.Messages.Safe())
            {
                logger.LogDebug(message);
            }

            return context;
        }

        public static NaosOptions AddServices(
            this NaosOptions naosOptions,
            Action<ServiceOptions> setupAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            setupAction?.Invoke(new ServiceOptions(naosOptions.Context));

            return naosOptions;
        }
    }
}

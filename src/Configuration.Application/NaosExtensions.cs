namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Newtonsoft.Json;
    using Console = Colorful.Console;

    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/>.
    /// </summary>
    public static class NaosExtensions
    {
        /// <summary>
        /// Adds required services to support the Discovery functionality.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="product"></param>
        /// <param name="capability"></param>
        /// <param name="tags"></param>
        /// <param name="optionsAction"></param>
        /// <param name="environment"></param>
        /// <param name="section"></param>
        public static INaosBuilderContext AddNaos(
            this IServiceCollection services,
            IConfiguration configuration,
            string product = null,
            string capability = null,
            string[] tags = null,
            Action<NaosServicesContextOptions> optionsAction = null,
            string environment = null,
            string section = "naos")
        {
            Console.WriteLine("--- naos service start", System.Drawing.Color.LimeGreen);
            EnsureArg.IsNotNull(services, nameof(services));

            var naosConfiguration = configuration?.GetSection(section).Get<NaosConfiguration>();
            var context = new NaosBuilderContext
            {
                Services = services,
                Configuration = configuration,
                Environment = environment ?? Environment.GetEnvironmentVariable(EnvironmentKeys.Environment) ?? "Production",
                Descriptor = new Naos.Foundation.ServiceDescriptor(
                    product ?? naosConfiguration.Product,
                    capability ?? naosConfiguration.Product,
                    tags: tags ?? naosConfiguration.Tags),
            };
            context.Messages.Add($"{LogKeys.Startup} naos services builder: naos services added");
            //context.Services.AddSingleton(new NaosFeatureInformation { Name = "Naos", EchoRoute = "naos/servicecontext/echo" });

            // TODO: optional or provide own settings?
            JsonConvert.DefaultSettings = DefaultJsonSerializerSettings.Create;

            optionsAction?.Invoke(new NaosServicesContextOptions(context));

            try
            {
                var logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("Naos");
                foreach (var message in context.Messages.Safe())
                {
                    logger.LogDebug(message);
                }
            }
            catch (InvalidOperationException)
            {
                // no loggerfactory registered
                services.AddScoped<ILoggerFactory>(sp => new LoggerFactory());
            }

            return context;
        }

        public static NaosServicesContextOptions AddModules(
            this NaosServicesContextOptions naosOptions,
            Action<ModuleOptions> optionsAction = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            optionsAction?.Invoke(new ModuleOptions(naosOptions.Context));

            return naosOptions;
        }

        public static NaosServicesContextOptions AddModule<TModule>(
            this NaosServicesContextOptions naosOptions,
            string section = null)
            where TModule : class
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(naosOptions.Context, nameof(naosOptions.Context));

            // TODO: create T instance with Factory.Creat<T>() and call inst.AddModule(options) << see CompositionRoot examples
            var module = Factory<TModule>.Create();
            //if (section.IsNullOrEmpty())
            //{
            //    module.Configure(new ModuleOptions(naosOptions.Context));
            //}
            //else
            //{
            //    module.Configure(new ModuleOptions(naosOptions.Context), section);
            //}

            return naosOptions;
        }
    }
}

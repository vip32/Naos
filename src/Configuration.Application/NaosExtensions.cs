namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
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
        /// <param name="product"></param>
        /// <param name="capability"></param>
        /// <param name="tags"></param>
        /// <param name="optionsAction"></param>
        /// <param name="environment"></param>
        /// <param name="section"></param>
        public static INaosBuilderContext AddNaos(
        this IServiceCollection services,
        string product = null,
        string capability = null,
        string[] tags = null,
        Action<NaosServicesContextOptions> optionsAction = null,
        string environment = null,
        string section = "naos")
        {
            Console.WriteLine("--- naos service start", System.Drawing.Color.LimeGreen);
            EnsureArg.IsNotNull(services, nameof(services));

            //using var serviceProvider = services.BuildServiceProvider();
            var configuration = /*serviceProvider.GetService<IConfiguration>() ??*/ NaosConfigurationFactory.Create();

            services.AddSingleton(configuration);
            services.AddMediatr();
            services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true); // https://andrewlock.net/new-in-aspnetcore-3-structured-logging-for-startup-messages/

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

            foreach(var provider in configuration?.Providers.Safe())
            {
                context.Messages.Add($"naos services builder: configuration provider added (type={provider.GetType().Name})");
            }

            context.Messages.Add("naos services builder: naos services added");
            //context.Services.AddSingleton(new NaosFeatureInformation { Name = "Naos", EchoRoute = "naos/servicecontext/echo" });

            // TODO: optional or provide own settings?
            JsonConvert.DefaultSettings = DefaultJsonSerializerSettings.Create;

            optionsAction?.Invoke(new NaosServicesContextOptions(context));

            AddConfigurationHealthChecks(services, configuration);
            LogStartupMessages(services, context);

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

        private static void LogStartupMessages(IServiceCollection services, NaosBuilderContext context)
        {
            try
            {
                var logger = services.BuildServiceProvider().GetService<ILoggerFactory>().CreateLogger("Naos");
                logger?.LogInformation($"{{LogKey:l}} service descriptor: {context.Descriptor} [{context.Descriptor.Tags.ToString("|")}]", LogKeys.Startup);
                foreach (var message in context.Messages.Safe())
                {
                    logger?.LogDebug($"{{LogKey:l}} {message.Replace("{", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("}", string.Empty, StringComparison.OrdinalIgnoreCase):l}", LogKeys.Startup);
                }
            }
            catch (InvalidOperationException)
            {
                // no loggerfactory registered
                services.AddScoped((Func<IServiceProvider, ILoggerFactory>)(sp => new LoggerFactory()));
            }
        }

        private static void AddConfigurationHealthChecks(IServiceCollection services, IConfiguration configuration)
        {
            if (configuration[ConfigurationKeys.AzureKeyVaultEnabled].ToBool()
                && !configuration[ConfigurationKeys.AzureKeyVaultName].IsNullOrEmpty()
                && !configuration[ConfigurationKeys.AzureKeyVaultClientId].IsNullOrEmpty()
                && !configuration[ConfigurationKeys.AzureKeyVaultClientSecret].IsNullOrEmpty())
            {
                services.AddHealthChecks()
                    .AddAzureKeyVault(s => s
                        .UseClientSecrets(configuration[ConfigurationKeys.AzureKeyVaultClientId], configuration[ConfigurationKeys.AzureKeyVaultClientSecret])
                        .UseKeyVaultUrl($"https://{configuration[ConfigurationKeys.AzureKeyVaultName]}.vault.azure.net/"), "configuration-keyvault", tags: new[] { "naos" });
            }

            // TODO: check other configuration providers here
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Options;
    using Naos.Foundation;

    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="IOptions{TOptions}"/> and <typeparamref name="TOptions"/> to the services container.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The same services collection.</returns>
        public static IServiceCollection ConfigureSingleton<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TOptions : class, new()
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            return services
                .Configure<TOptions>(configuration)
                .AddSingleton(x => x.GetRequiredService<IOptions<TOptions>>().Value);
        }

        /// <summary>
        /// Registers <see cref="IOptions{TOptions}"/> and <typeparamref name="TOptions"/> to the services container.
        /// Also runs data annotation validation.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The same services collection.</returns>
        public static IServiceCollection ConfigureAndValidateSingleton<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TOptions : class, new()
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            services
                .AddOptions<TOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations();
            return services.AddSingleton(x => x.GetRequiredService<IOptions<TOptions>>().Value);
        }

        /// <summary>
        /// Registers <see cref="IOptions{TOptions}"/> and <typeparamref name="TOptions"/> to the services container.
        /// Also runs data annotation validation and custom validation using the default failure message.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="validation">The validation function.</param>
        /// <returns>The same services collection.</returns>
        public static IServiceCollection ConfigureAndValidateSingleton<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<TOptions, bool> validation)
            where TOptions : class, new()
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNull(validation, nameof(validation));

            services
                .AddOptions<TOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .Validate(validation);
            return services.AddSingleton(x => x.GetRequiredService<IOptions<TOptions>>().Value);
        }

        /// <summary>
        /// Registers <see cref="IOptions{TOptions}"/> and <typeparamref name="TOptions"/> to the services container.
        /// Also runs data annotation validation and custom validation.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="validation">The validation function.</param>
        /// <param name="failureMessage">The failure message to use when validation fails.</param>
        /// <returns>The same services collection.</returns>
        public static IServiceCollection ConfigureAndValidateSingleton<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<TOptions, bool> validation,
            string failureMessage)
            where TOptions : class, new()
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNull(validation, nameof(validation));
            EnsureArg.IsNotNullOrEmpty(failureMessage, nameof(failureMessage));

            services
                .AddOptions<TOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .Validate(validation, failureMessage);
            return services.AddSingleton(x => x.GetRequiredService<IOptions<TOptions>>().Value);
        }

        public static IServiceCollection AddMediatr(
            this IServiceCollection services,
            IEnumerable<string> assemblyBlackListPatterns = null)
        {
            if (services == null)
            {
                return services;
            }

            if (assemblyBlackListPatterns.IsNullOrEmpty())
            {
                assemblyBlackListPatterns = new[]
                {
                    "Microsoft.*",
                    "System.*",
                    "Naos.Foundation*"
                };
            }

            // find all assembly references (including unloaded), so mediatr can inspect all available assemblies
            var assemblies = DependencyContext.Default.RuntimeLibraries
                .SelectMany(l => l.GetDefaultAssemblyNames(DependencyContext.Default))
                .Where(a => !a.Name.EqualsPatternAny(assemblyBlackListPatterns))
                .Select(Assembly.Load)
                //.Where(a => !a.GlobalAssemblyCache)
                .ToArray();

            return services.AddMediatR(assemblies);
        }
    }
}
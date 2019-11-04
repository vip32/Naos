namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.ServiceContext.Application.Web;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables service context (descriptor) for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseServiceContext(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseNaosServiceContext(new ServiceContextMiddlewareOptions());
        }

        /// <summary>
        /// Enables service context (descriptor) for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        public static NaosApplicationContextOptions UseNaosServiceContext(this NaosApplicationContextOptions naosOptions, ServiceContextMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            if (naosOptions.Context.Application.ApplicationServices.GetService(typeof(Naos.Foundation.ServiceDescriptor)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddServiceContext method in ConfigureServices in the application startup code.");
            }

            naosOptions.Context.Application.UseMiddleware<ServiceContextMiddleware>(Options.Create(options));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: service context added");
            return naosOptions;
        }

        /// <summary>
        /// Enables poweredby response headers for the API request.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseServicePoweredBy(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseNaosServicePoweredBy(new ServicePoweredByMiddlewareOptions());
        }

        /// <summary>
        /// Enables poweredby response headers for the API request.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        public static NaosApplicationContextOptions UseNaosServicePoweredBy(this NaosApplicationContextOptions naosOptions, ServicePoweredByMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            naosOptions.Context.Application.UseMiddleware<ServicePoweredByMiddleware>(Options.Create(options));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: powered by added");
            return naosOptions;
        }
    }
}

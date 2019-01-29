namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands;
    using Naos.Core.ServiceContext.App.Web;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables service context (descriptor) for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosServiceContext(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosServiceContext(new ServiceContextMiddlewareOptions());
        }

        /// <summary>
        /// Enables service context (descriptor) for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosServiceContext(this IApplicationBuilder app, ServiceContextMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(ServiceDescriptor)) == null)
            {
                throw new InvalidOperationException($"Unable to find the required services. You must call the AddServiceContext method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<ServiceContextMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Enables poweredby response headers for the API request.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosServicePoweredBy(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosServicePoweredBy(new ServicePoweredByMiddlewareOptions());
        }

        /// <summary>
        /// Enables poweredby response headers for the API request.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosServicePoweredBy(this IApplicationBuilder app, ServicePoweredByMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            return app.UseMiddleware<ServicePoweredByMiddleware>(Options.Create(options));
        }
    }
}

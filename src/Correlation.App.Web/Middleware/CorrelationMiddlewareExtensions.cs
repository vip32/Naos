namespace Naos.Core.Correlation.App.Web
{
    using System;
    using EnsureThat;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Options;
    using Naos.Core.Correlation.Domain;

    /// <summary>
    /// Extension methods for the CorrelationIdMiddleware.
    /// </summary>
    public static class CorrelationMiddlewareExtensions
    {
        /// <summary>
        /// Enables correlation IDs for the request.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCorrelation(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosCorrelation(new CorrelationOptions());
        }

        /// <summary>
        /// Enables correlation IDs for the request.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="header">The header field name to use for the correlation ID.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCorrelation(this IApplicationBuilder app, string header)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosCorrelation(new CorrelationOptions
            {
                Header = header
            });
        }

        /// <summary>
        /// Enables correlation IDs for the request.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCorrelation(this IApplicationBuilder app, CorrelationOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddCorrelationId method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<CorrelationMiddleware>(Options.Create(options));
        }
    }
}

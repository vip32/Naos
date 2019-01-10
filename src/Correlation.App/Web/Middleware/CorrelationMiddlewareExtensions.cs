namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.App.Correlation.App.Web;
    using Naos.Core.Correlation.App;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class CorrelationMiddlewareExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCorrelation(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosCorrelation(new CorrelationMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCorrelation(this IApplicationBuilder app, CorrelationMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException($"Unable to find the required services. You must call the {nameof(Microsoft.Extensions.DependencyInjection.ServiceRegistrations.AddNaosCorrelation)} method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<CorrelationMiddleware>(Options.Create(options));
        }
    }
}

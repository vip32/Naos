namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.Correlation.App.Web;
    using Naos.Core.RequestCorrelation.App;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosRequestCorrelation(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosCorrelation(new RequestCorrelationMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCorrelation(this IApplicationBuilder app, RequestCorrelationMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException($"Unable to find the required services. You must call the {nameof(Microsoft.Extensions.DependencyInjection.ServiceExtensions.AddNaosRequestCorrelation)} method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<RequestCorrelationMiddleware>(Options.Create(options));
        }
    }
}

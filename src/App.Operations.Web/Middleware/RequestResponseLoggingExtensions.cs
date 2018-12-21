namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.App.Operations.Web;
    using Naos.Core.Common.Web;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class RequestResponseLoggingExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosOperationsRequestResponseLogging(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosOperationsRequestResponseLogging(new RequestResponseLoggingOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosOperationsRequestResponseLogging(this IApplicationBuilder app, RequestResponseLoggingOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddNaosCorrelation method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<RequestResponseLoggingMiddleware>(Options.Create(options));
        }
    }
}

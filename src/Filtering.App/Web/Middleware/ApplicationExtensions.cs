namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.Filtering.App.Web;
    using Naos.Core.RequestFiltering.App;

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
        public static IApplicationBuilder UseNaosRequestFiltering(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosFiltering(new RequestFilterMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosFiltering(this IApplicationBuilder app, RequestFilterMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(IFilterContextFactory)) == null)
            {
                throw new InvalidOperationException($"Unable to find the required services. You must call the AddRequestFiltering method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<RequestFilterMiddleware>(Options.Create(options));
        }
    }
}

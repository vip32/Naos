namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.App.Criteria.App.Web;
    using Naos.Core.Filtering.App;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class FilterMiddlewareExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCriteria(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosCriteria(new FilterMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosCriteria(this IApplicationBuilder app, FilterMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            if (app.ApplicationServices.GetService(typeof(IFilterContextFactory)) == null)
            {
                throw new InvalidOperationException($"Unable to find the required services. You must call the {nameof(Microsoft.Extensions.DependencyInjection.ServiceExtensions.AddNaosFiltering)} method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<FilterMiddleware>(Options.Create(options));
        }
    }
}

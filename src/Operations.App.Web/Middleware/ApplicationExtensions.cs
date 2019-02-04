namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.Operations.App.Web;

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

            return app.UseMiddleware<RequestResponseLoggingMiddleware>(Options.Create(options));
        }
    }
}

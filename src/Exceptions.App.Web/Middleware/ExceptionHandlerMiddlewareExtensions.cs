namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.Extensions.Options;
    using Naos.Core.ServiceExceptions.App.Web;

    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseNaosExceptionHandling(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseNaosExceptionHandling(new ExceptionHandlerMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNaosExceptionHandling(this IApplicationBuilder app, ExceptionHandlerMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(options, nameof(options));

            return app.UseMiddleware<Naos.Core.ServiceExceptions.App.Web.ExceptionHandlerMiddleware>(Options.Create(options));
        }
    }
}

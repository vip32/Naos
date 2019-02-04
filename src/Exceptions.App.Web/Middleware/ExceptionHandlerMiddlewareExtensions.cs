namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.Exceptions.Web;

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

            return app.UseMiddleware<ExceptionHandlerMiddleware>(Options.Create(options));
        }
    }
}

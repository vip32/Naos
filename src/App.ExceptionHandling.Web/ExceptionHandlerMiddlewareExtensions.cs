namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Naos.Core.App.Exceptions.Web;

    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseNaosExceptionHandling(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            return app.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Naos.Core.App.Exceptions.Web;
    using SimpleInjector;

    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseNaosExceptionHandling(this IApplicationBuilder app, Container container)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(container, nameof(container));

            return SimpleInjectorAspNetCoreIntegrationExtensions.UseMiddleware<ExceptionHandlerMiddleware>(app, container);
            //return app.UseMiddleware<ExceptionHandlerMiddleware>(container);
        }
    }
}

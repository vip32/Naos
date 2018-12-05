namespace Naos.Core.App.Exceptions.Web
{
    using EnsureThat;
    using Microsoft.AspNetCore.Builder;
    using SimpleInjector;

    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseNaosExceptionHandling(this IApplicationBuilder app, Container container)
        {
            EnsureArg.IsNotNull(app, nameof(app));
            EnsureArg.IsNotNull(container, nameof(container));

            return app.UseMiddleware<ExceptionHandlerMiddleware>(container);
        }
    }
}

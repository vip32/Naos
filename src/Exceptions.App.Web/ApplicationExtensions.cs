namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.ServiceExceptions.App.Web;
    using Naos.Foundation;

    public static class ApplicationExtensions
    {
        public static NaosApplicationContextOptions UseServiceExceptions(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseNaosExceptionHandling(new ExceptionHandlerMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        public static NaosApplicationContextOptions UseNaosExceptionHandling(this NaosApplicationContextOptions naosOptions, ExceptionHandlerMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            naosOptions.Context.Application.UseMiddleware<ExceptionHandlerMiddleware>(Options.Create(options));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: service exceptions added");
            return naosOptions;
        }
    }
}

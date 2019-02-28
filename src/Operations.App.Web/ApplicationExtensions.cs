namespace Microsoft.AspNetCore.Builder
{
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.Operations.App.Web;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="requestLoggingMiddlewareOptions"></param>
        /// <param name="requestStorageMiddlewareOptions"></param>
        /// <returns></returns>
        public static NaosApplicationContextOptions UseOperations(this NaosApplicationContextOptions naosOptions,
            RequestLoggingMiddlewareOptions requestLoggingMiddlewareOptions = null,
            RequestStorageMiddlewareOptions requestStorageMiddlewareOptions = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            naosOptions.Context.Application
                .UseMiddleware<RequestLoggingMiddleware>(
                    Options.Create(requestLoggingMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestLoggingMiddlewareOptions>() ?? new RequestLoggingMiddlewareOptions()))
                .UseMiddleware<RequestStorageMiddleware>(
                    Options.Create(requestStorageMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestStorageMiddlewareOptions>() ?? new RequestStorageMiddlewareOptions()));
            naosOptions.Context.Messages.Add($"{LogEventKeys.Startup} naos application builder: operations added");

            var diagnosticListener = naosOptions.Context.Application.ApplicationServices.GetService<DiagnosticListener>();
            diagnosticListener?.SubscribeWithAdapter(new NaosDiagnosticListener(naosOptions.Context.Application.ApplicationServices.GetService<ILoggerFactory>()));

            return naosOptions;
        }
    }
}

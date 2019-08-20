namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.AspNetCore.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Core.Operations.App.Web;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="requestLoggingMiddlewareOptions"></param>
        /// <param name="requestStorageMiddlewareOptions"></param>
        public static NaosApplicationContextOptions UseOperationsLogging(
            this NaosApplicationContextOptions naosOptions,
            RequestLoggingMiddlewareOptions requestLoggingMiddlewareOptions = null,
            RequestStorageMiddlewareOptions requestStorageMiddlewareOptions = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            naosOptions.Context.Application.UseEndpointRouting(); // needed by middleware to get action/controller https://www.stevejgordon.co.uk/asp-net-core-first-look-at-global-routing-dispatcher
            naosOptions.Context.Application
                .UseMiddleware<RequestLoggingMiddleware>(
                    Options.Create(requestLoggingMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestLoggingMiddlewareOptions>() ?? new RequestLoggingMiddlewareOptions()))
                .UseMiddleware<RequestStorageMiddleware>(
                    Options.Create(requestStorageMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestStorageMiddlewareOptions>() ?? new RequestStorageMiddlewareOptions()));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: operations logging added");

            var diagnosticListener = naosOptions.Context.Application.ApplicationServices.GetService<DiagnosticListener>();
            diagnosticListener?.SubscribeWithAdapter(new NaosDiagnosticListener(naosOptions.Context.Application.ApplicationServices.GetService<ILoggerFactory>()));

            return naosOptions;
        }

        /// <summary>
        /// Enables tracing for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="requestTracingMiddlewareOptions"></param>
        public static NaosApplicationContextOptions UseOperationsTracing(
            this NaosApplicationContextOptions naosOptions,
            RequestTracingMiddlewareOptions requestTracingMiddlewareOptions = null)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            using (var scope = naosOptions.Context.Application.ApplicationServices.CreateScope())
            {
                if (scope.ServiceProvider.GetService(typeof(ITracer)) == null) // resolve scoped service
                {
                    throw new InvalidOperationException("Unable to find the required services. You must call the AddTracing method in ConfigureServices in the application startup code.");
                }
            }

            naosOptions.Context.Application.UseEndpointRouting(); // needed by middleware to get action/controller https://www.stevejgordon.co.uk/asp-net-core-first-look-at-global-routing-dispatcher
            naosOptions.Context.Application.UseMiddleware<RequestTracingMiddleware>(
                    Options.Create(requestTracingMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestTracingMiddlewareOptions>() ?? new RequestTracingMiddlewareOptions()));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: operations tracing added");

            return naosOptions;
        }
    }
}

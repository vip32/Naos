﻿namespace Microsoft.AspNetCore.Builder
{
    using System;
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Operations.App.Web;
    using Naos.Tracing.Domain;

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

            //naosOptions.Context.Application.UseEndpointRouting(); // needed by middleware to get action/controller https://www.stevejgordon.co.uk/asp-net-core-first-look-at-global-routing-dispatcher
            naosOptions.Context.Application
                .UseMiddleware<RequestLoggingMiddleware>(
                    Options.Create(requestLoggingMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestLoggingMiddlewareOptions>() ?? new RequestLoggingMiddlewareOptions()))
                .UseMiddleware<RequestStorageMiddleware>(
                    Options.Create(requestStorageMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestStorageMiddlewareOptions>() ?? new RequestStorageMiddlewareOptions()));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: operations request logging added");

            var diagnosticListener = naosOptions.Context.Application.ApplicationServices.GetService<DiagnosticListener>();
            diagnosticListener?.SubscribeWithAdapter(new NaosDiagnosticListener(naosOptions.Context.Application.ApplicationServices.GetService<ILoggerFactory>()));

            return naosOptions;
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Operations.Application.Web;

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
            naosOptions.Context.Messages.Add("naos application builder: operations request logging added");

            var diagnosticListener = naosOptions.Context.Application.ApplicationServices.GetService<DiagnosticListener>();
            diagnosticListener?.SubscribeWithAdapter(new NaosDiagnosticListener(naosOptions.Context.Application.ApplicationServices.GetService<ILoggerFactory>()));

            return naosOptions;
        }

        public static NaosApplicationContextOptions UseOperationsHealth(
            this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            //naosOptions.Context.Application.UseEndpointRouting(); // needed by middleware to get action/controller https://www.stevejgordon.co.uk/asp-net-core-first-look-at-global-routing-dispatcher
            //naosOptions.Context.Application.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapHealthChecks("/health", new HealthCheckOptions
            //    {
            //        ResponseWriter = HealthReportResponseWriter.Write
            //    });
            //});

            // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1
            naosOptions.Context.Application.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = HealthReportResponseWriter.Write // or use HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
            });
            naosOptions.Context.Application.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = HealthReportResponseWriter.Write // or use HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
            });
            naosOptions.Context.Application.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
                ResponseWriter = HealthReportResponseWriter.Write // or use HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
            });

            naosOptions.Context.Messages.Add("naos application builder: operations health added");

            return naosOptions;
        }
    }
}

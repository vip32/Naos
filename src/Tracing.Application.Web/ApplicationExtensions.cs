namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Tracing.Application.Web;
    using Naos.Tracing.Domain;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
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

            //naosOptions.Context.Application.UseEndpointRouting(); // needed by middleware to get action/controller https://www.stevejgordon.co.uk/asp-net-core-first-look-at-global-routing-dispatcher
            naosOptions.Context.Application.UseMiddleware<RequestTracingMiddleware>(
                    Options.Create(requestTracingMiddlewareOptions ?? naosOptions.Context.Application.ApplicationServices.GetService<RequestTracingMiddlewareOptions>() ?? new RequestTracingMiddlewareOptions()));
            naosOptions.Context.Messages.Add("naos application builder: operations request tracing added");

            return naosOptions;
        }
    }
}

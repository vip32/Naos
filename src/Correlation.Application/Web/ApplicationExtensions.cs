namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Commands.Correlation.Application.Web;
    using Naos.RequestCorrelation.Application;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseRequestCorrelation(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseNaosCorrelation(new RequestCorrelationMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        public static NaosApplicationContextOptions UseNaosCorrelation(this NaosApplicationContextOptions naosOptions, RequestCorrelationMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            if (naosOptions.Context.Application.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddRequestCorrelation method in ConfigureServices in the application startup code.");
            }

            naosOptions.Context.Application.UseMiddleware<RequestCorrelationMiddleware>(Options.Create(options));
            naosOptions.Context.Messages.Add("naos application builder: request correlation added");
            return naosOptions;
        }
    }
}

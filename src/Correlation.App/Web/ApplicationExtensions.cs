namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.Commands.Correlation.App.Web;
    using Naos.Core.Common;
    using Naos.Core.RequestCorrelation.App;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        public static NaosApplicationContextOptions UseNaosCorrelation(this NaosApplicationContextOptions naosOptions, RequestCorrelationMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            if(naosOptions.Context.Application.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddRequestCorrelation method in ConfigureServices in the application startup code.");
            }

            naosOptions.Context.Application.UseMiddleware<RequestCorrelationMiddleware>(Options.Create(options));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: request correlation added");
            return naosOptions;
        }
    }
}

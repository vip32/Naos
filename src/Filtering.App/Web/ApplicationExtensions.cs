namespace Microsoft.AspNetCore.Builder
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Core.Common;
    using Naos.Core.RequestFiltering.App.Web;

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
        public static NaosApplicationContextOptions UseRequestFiltering(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseNaosFiltering(new RequestFilterMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static NaosApplicationContextOptions UseNaosFiltering(this NaosApplicationContextOptions naosOptions, RequestFilterMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            if(naosOptions.Context.Application.ApplicationServices.GetService(typeof(IFilterContextFactory)) == null)
            {
                throw new InvalidOperationException($"Unable to find the required services. You must call the AddRequestFiltering method in ConfigureServices in the application startup code.");
            }

            naosOptions.Context.Application.UseMiddleware<RequestFilterMiddleware>(Options.Create(options));
            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: request filtering added");
            return naosOptions;
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Naos.Authentication.Application.Web;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
#if NETCOREAPP3_1

        /// <summary>
        /// Force authentication for all endpoints (mvc + custom middleware)
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseAuthenticationChallenge(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseAuthenticationChallenge(new OidcAuthenticationChallengeMiddlewareOptions());
        }

        /// <summary>
        /// Force authentication for all endpoints (mvc + custom middleware)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAuthenticationChallenge(this IApplicationBuilder builder)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));

            return builder.UseAuthenticationChallenge(new OidcAuthenticationChallengeMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        public static NaosApplicationContextOptions UseAuthenticationChallenge(this NaosApplicationContextOptions naosOptions, OidcAuthenticationChallengeMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            var provider = naosOptions.Context.Application.ApplicationServices.GetService<IAuthenticationSchemeProvider>();
            if (provider != null)
            {
                if (provider.GetDefaultChallengeSchemeAsync().Result?.Name == Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
                {
                    naosOptions.Context.Application.UseMiddleware<OidcAuthenticationChallengeMiddleware>(Options.Create(options));
                    naosOptions.Context.Messages.Add("naos application builder: authentication challenge");
                }

                // TODO: register other middleware for different authentication schemes (easyauth?)
            }

            return naosOptions;
        }

        public static IApplicationBuilder UseAuthenticationChallenge(this IApplicationBuilder builder, OidcAuthenticationChallengeMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));
            EnsureArg.IsNotNull(options, nameof(options));

            var provider = builder.ApplicationServices.GetService<IAuthenticationSchemeProvider>();
            if (provider != null)
            {
                if (provider.GetDefaultChallengeSchemeAsync().Result?.Name == AuthenticationKeys.OidcScheme)
                {
                    builder.UseMiddleware<OidcAuthenticationChallengeMiddleware>(Options.Create(options));
                    //naosOptions.Context.Messages.Add($"naos application builder: authentication challenge");
                }

                // TODO: register other middleware for different authentication schemes (easyauth?)
            }

            return builder;
        }
#endif
    }
}

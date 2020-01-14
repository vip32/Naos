namespace Microsoft.AspNetCore.Builder
{
    using EnsureThat;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Naos.Authentication.Application.Web;

    /// <summary>
    /// Extension methods for the correlation middleware.
    /// </summary>
    public static class ApplicationExtensions
    {
#if NETCOREAPP3_1

        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        public static NaosApplicationContextOptions UseAuthenticationChallenge(this NaosApplicationContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            return naosOptions.UseAuthenticationChallenge(new AuthenticationChallengeMiddlewareOptions());
        }

        /// <summary>
        /// Enables correlation/request ids for the API request/responses.
        /// </summary>
        /// <param name="naosOptions"></param>
        /// <param name="options"></param>
        public static NaosApplicationContextOptions UseAuthenticationChallenge(this NaosApplicationContextOptions naosOptions, AuthenticationChallengeMiddlewareOptions options)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));
            EnsureArg.IsNotNull(options, nameof(options));

            var authenticationOptions = naosOptions.Context.Application.ApplicationServices.GetService<IAuthenticationSchemeProvider>();
            if (authenticationOptions != null)
            {
                if(authenticationOptions.GetDefaultChallengeSchemeAsync().Result.Name == Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
                {
                    naosOptions.Context.Application.UseMiddleware<AuthenticationChallengeMiddleware>(Options.Create(options));
                    naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos application builder: authentication challenge");
                }
            }

            return naosOptions;
        }
#endif
    }
}

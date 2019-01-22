namespace Naos.Core.Authentication.App.Web
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Extension methods to add API Key authentication capabilities to the HTTP application pipeline.
    /// </summary>
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
            => builder.AddApiKey(AuthenticationKeys.ApiKeyScheme, _ => { });

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions> configureOptions)
            => builder.AddApiKey(AuthenticationKeys.ApiKeyScheme, configureOptions);

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyAuthenticationOptions> configureOptions)
            => builder.AddApiKey(authenticationScheme, "API Key", configureOptions);

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<ApiKeyAuthenticationOptions> configureOptions)
            => builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}

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

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, Action<AuthenticationHandlerOptions> options)
            => builder.AddApiKey(AuthenticationKeys.ApiKeyScheme, options);

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, Action<AuthenticationHandlerOptions> options)
            => builder.AddApiKey(authenticationScheme, authenticationScheme, options);

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<AuthenticationHandlerOptions> options)
            => builder.AddScheme<AuthenticationHandlerOptions, ApiKeyAuthenticationHandler>(authenticationScheme, displayName, options);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
            => builder.AddBasic(AuthenticationKeys.BasicScheme, _ => { });

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, Action<AuthenticationHandlerOptions> options)
            => builder.AddBasic(AuthenticationKeys.BasicScheme, options);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, Action<AuthenticationHandlerOptions> options)
            => builder.AddBasic(authenticationScheme, authenticationScheme, options);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<AuthenticationHandlerOptions> options)
            => builder.AddScheme<AuthenticationHandlerOptions, BasicAuthenticationHandler>(authenticationScheme, displayName, options);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder)
            => builder.AddEasyAuth(AuthenticationKeys.EasyAuthScheme, _ => { });

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, Action<AuthenticationHandlerOptions> options)
            => builder.AddEasyAuth(AuthenticationKeys.EasyAuthScheme, options);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, Action<AuthenticationHandlerOptions> options)
            => builder.AddEasyAuth(authenticationScheme, authenticationScheme, options);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<AuthenticationHandlerOptions> options)
            => builder.AddScheme<AuthenticationHandlerOptions, EasyAuthAuthenticationHandler>(authenticationScheme, displayName, options);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Authentication.App.Web;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        //o =>
        //{
        //    o.Events = new AuthenticationHandlerEvents // optional
        //    {
        //        OnChallenge = context =>
        //        {
        //            Trace.TraceError("ohoh api");
        //            return Task.CompletedTask;
        //        }
        //    };
        //}
        public static NaosOptions AddAuthenticationApiKeyStatic(
            this NaosOptions naosOptions,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:apikey:static")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            var serviceConfiguration = naosOptions.Context.Configuration.GetSection(section).Get<ApiKeyStaticValidationServiceConfiguration>();
            naosOptions.Context.Services.AddSingleton<IAuthenticationService, ApiKeyStaticValidationService>(sp => new ApiKeyStaticValidationService(serviceConfiguration));

            naosOptions.Context.Services
                .AddAuthentication(AuthenticationKeys.ApiKeyScheme)
                .AddApiKey(options);

            return naosOptions;
        }

        public static NaosOptions AddAuthenticationBasicStatic(
            this NaosOptions naosOptions,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:basic:static")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            var serviceConfiguration = naosOptions.Context.Configuration.GetSection(section).Get<BasicStaticValidationServiceConfiguration>();
            naosOptions.Context.Services.AddSingleton<IAuthenticationService, BasicStaticValidationService>(sp => new BasicStaticValidationService(serviceConfiguration));

            naosOptions.Context.Services
                .AddAuthentication(AuthenticationKeys.BasicScheme)
                .AddBasic(options);

            return naosOptions;
        }
    }
}

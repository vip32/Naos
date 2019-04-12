namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Authentication.App.Web;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static NaosServicesContextOptions AddAuthenticationApiKeyStatic(
            this NaosServicesContextOptions naosOptions,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:apikey:static")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            var serviceConfiguration = naosOptions.Context.Configuration.GetSection(section).Get<ApiKeyStaticValidationServiceConfiguration>();
            naosOptions.Context.Services.AddSingleton<IAuthenticationService, ApiKeyStaticValidationService>(sp => new ApiKeyStaticValidationService(serviceConfiguration));

            naosOptions.Context.Services
                .AddAuthentication(AuthenticationKeys.ApiKeyScheme)
                .AddApiKey(options);

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: authentication added (type=ApiKeyStatic)");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Authentication", Description = "ApiKeyStatic", EchoRoute = "api/echo/authentication" });

            return naosOptions;
        }

        public static NaosServicesContextOptions AddAuthenticationBasicStatic(
            this NaosServicesContextOptions naosOptions,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:basic:static")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            var serviceConfiguration = naosOptions.Context.Configuration.GetSection(section).Get<BasicStaticValidationServiceConfiguration>();
            naosOptions.Context.Services.AddSingleton<IAuthenticationService, BasicStaticValidationService>(sp => new BasicStaticValidationService(serviceConfiguration));

            naosOptions.Context.Services
                .AddAuthentication(AuthenticationKeys.BasicScheme)
                .AddBasic(options);

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: authentication added (type=BasicStatic)");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Authentication", Description = "BasicStatic", EchoRoute = "api/echo/authentication" });

            return naosOptions;
        }
    }
}

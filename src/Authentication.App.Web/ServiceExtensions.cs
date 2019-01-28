namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Authentication.App.Web;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosAuthenticationApiKeyStatic(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthenticationHandlerOptions> options = null,
        string section = "naos:authentication:apikey:static")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var serviceConfiguration = configuration.GetSection(section).Get<ApiKeyStaticValidationServiceConfiguration>();
            services.AddSingleton<IAuthenticationService, ApiKeyStaticValidationService>(sp => new ApiKeyStaticValidationService(serviceConfiguration));

            services
                .AddAuthentication(AuthenticationKeys.ApiKeyScheme)
                .AddApiKey(options);

            return services;
        }

        public static IServiceCollection AddNaosAuthenticationBasicStatic(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthenticationHandlerOptions> options = null,
        string section = "naos:authentication:basic:static")
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var serviceConfiguration = configuration.GetSection(section).Get<BasicStaticValidationServiceConfiguration>();
            services.AddSingleton<IAuthenticationService, BasicStaticValidationService>(sp => new BasicStaticValidationService(serviceConfiguration));

            services
                .AddAuthentication(AuthenticationKeys.BasicScheme)
                .AddBasic(options);

            return services;
        }
    }
}

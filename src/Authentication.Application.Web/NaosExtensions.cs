namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Policy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Authentication.Application.Web;
    using Naos.Configuration.Application;
    using Naos.Foundation;

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

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: authentication added (type={AuthenticationKeys.ApiKeyScheme})");
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

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: authentication added (type={AuthenticationKeys.BasicScheme})");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Authentication", Description = "BasicStatic", EchoRoute = "api/echo/authentication" });

            return naosOptions;
        }

        public static NaosServicesContextOptions AddEasyAuthentication(
            this NaosServicesContextOptions naosOptions,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:easyauth")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            var configuration = naosOptions.Context.Configuration.GetSection(section).Get<EasyAuthConfiguration>();
            var handlerOptions = new AuthenticationHandlerOptions();
            options?.Invoke(handlerOptions);

            naosOptions.Context.Services
                .AddAuthorization()
                .AddScoped<IPolicyEvaluator>(sp => new EasyAuthPolicyEvaluator(
                    sp.GetRequiredService<IAuthorizationService>(),
                    handlerOptions.Provider.EmptyToNull() ?? configuration.Provider.EmptyToNull() ?? EasyAuthProviders.AzureActiveDirectory))
                .AddAuthentication(AuthenticationKeys.EasyAuthScheme)
                .AddEasyAuth(options);

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: authentication added (type={AuthenticationKeys.EasyAuthScheme})");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Authentication", Description = "EasyAuth", EchoRoute = "api/echo/authentication" });

            return naosOptions;
        }

#if NETCOREAPP3_1
        public static NaosServicesContextOptions AddOidcAuthentication(
            this NaosServicesContextOptions naosOptions,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:oidc")
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            var configuration = naosOptions.Context.Configuration.GetSection(section).Get<OidcConfiguration>();
            var handlerOptions = new AuthenticationHandlerOptions();
            options?.Invoke(handlerOptions);

            naosOptions.Context.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = configuration.Authority;
                options.ClientId = configuration.ClientId;
                options.ClientSecret = configuration.ClientSecret;
                options.SaveTokens = true;
                options.ResponseType = IdentityModel.Protocols.OpenIdConnect.OpenIdConnectResponseType.Code; // configuration.ResponseType;
                options.RequireHttpsMetadata = false; // dev only
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("claims");
                options.SaveTokens = true;
                //options.Events = new OpenIdConnectEvents
                //{
                //    OnTokenResponseReceived = async ctx =>
                //    {
                //        var a = ctx.Principal;
                //    },
                //    OnAuthorizationCodeReceived = async ctx =>
                //    {
                //        var a = ctx.Principal;
                //    }
                //};

                options.TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "groups",
                    ValidateIssuer = true
                };
            });
            naosOptions.Context.Services.AddAuthorization();

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: authentication added (type={AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme})");
            naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Authentication", Description = $"{AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme}", EchoRoute = "api/echo/authentication" });

            return naosOptions;
        }
#endif
    }
}

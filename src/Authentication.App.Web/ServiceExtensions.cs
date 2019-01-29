namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Authentication.App.Web;

    public static class ServiceExtensions
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
        public static ServiceConfigurationContext AddAuthenticationApiKeyStatic(
            this ServiceConfigurationContext context,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:apikey:static")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            var serviceConfiguration = context.Configuration.GetSection(section).Get<ApiKeyStaticValidationServiceConfiguration>();
            context.Services.AddSingleton<IAuthenticationService, ApiKeyStaticValidationService>(sp => new ApiKeyStaticValidationService(serviceConfiguration));

            context.Services
                .AddAuthentication(AuthenticationKeys.ApiKeyScheme)
                .AddApiKey(options);

            return context;
        }

        public static ServiceConfigurationContext AddAuthenticationBasicStatic(
            this ServiceConfigurationContext context,
            Action<AuthenticationHandlerOptions> options = null,
            string section = "naos:authentication:basic:static")
        {
            EnsureArg.IsNotNull(context, nameof(context));

            var serviceConfiguration = context.Configuration.GetSection(section).Get<BasicStaticValidationServiceConfiguration>();
            context.Services.AddSingleton<IAuthenticationService, BasicStaticValidationService>(sp => new BasicStaticValidationService(serviceConfiguration));

            context.Services
                .AddAuthentication(AuthenticationKeys.BasicScheme)
                .AddBasic(options);

            return context;
        }
    }
}

namespace Naos.Application.Web
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EnsureThat;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Authentication.Application.Web;
    using Naos.Configuration.Application;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Security;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static IMvcBuilder AddNaos(
            this IMvcBuilder mvcBuilder,
            Action<NaosMvcOptions> optionsAction = null)
        {
            var options = new NaosMvcOptions();
            optionsAction?.Invoke(options);

            // add the generic repo controllers for all registrations
            if (!options.ControllerRegistrations.IsNullOrEmpty())
            {
                mvcBuilder
                    .AddMvcOptions(o =>
                    {
                        o.Filters.Add<OperationCancelledExceptionFilterAttribute>();
                        o.Conventions.Add(new GeneratedControllerRouteConvention());
                    })
                    .ConfigureApplicationPartManager(o => o
                        .FeatureProviders.Add(
                            new GenericRepositoryControllerFeatureProvider(
                                options.ControllerRegistrations)));

                mvcBuilder.Services.AddSingleton<IOperationProcessor>(new GenericRepositoryControllerOperationProcessor()); // needed for swagger generation (for each controller registration)
            }

            mvcBuilder.AddControllersAsServices(); // needed to resolve controllers through di https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
            //mvcBuilder.AddJsonOptions(o => o.AddDefaultJsonSerializerSettings(options.JsonSerializerSettings));

            return mvcBuilder;
        }

        public static NaosServicesContextOptions AddSwaggerDocumentation(
            this NaosServicesContextOptions naosOptions)
        {
            EnsureArg.IsNotNull(naosOptions, nameof(naosOptions));

            naosOptions.Context.Services
                .AddSwaggerDocument((c, sp) => // TODO: replace with .AddOpenApiDocument, but currently has issues with example model generation in UI
                {
                    // TODO: AddNaos.AddSwaggerDocument() ^^^^
                    c.SerializerSettings = DefaultJsonSerializerSettings.Create();
                    // find all processors which are registerd by various naos features (Command RequestDispatcher/ControllerRegistrations)
                    foreach (var documentProcessor in sp.GetServices<IDocumentProcessor>())
                    {
                        c.DocumentProcessors.Add(documentProcessor);
                    }

                    foreach (var operationProcessor in sp.GetServices<IOperationProcessor>())
                    {
                        c.OperationProcessors.Add(operationProcessor);
                    }

                    //c.DocumentProcessors.Add(
                    //    new SecurityDefinitionAppender("Bearer", new OpenApiSecurityScheme
                    //    {
                    //        Name = "Authorization",
                    //        Description = "Authorization header using the ApiKey scheme. Example: \"Authorization: ApiKey {value}\"",
                    //        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    //        Type = NSwag.OpenApiSecuritySchemeType.ApiKey // Oauth2/OIDC?
                    //    }));

                    //c.DocumentProcessors.Add(new RequestCommandRegistrationDocumentProcessor(sp.GetServices<RequestCommandRegistration>()));
                    //c.OperationProcessors.Add(new GenericRepositoryControllerOperationProcessor());
                    c.OperationProcessors.Add(new ApiVersionProcessor());
                    c.PostProcess = document =>
                    {
                        var descriptor = sp.GetService<Foundation.ServiceDescriptor>();
                        document.Info.Version = descriptor?.Version.EmptyToNull() ?? "v1";
                        document.Info.Title = descriptor?.Name.EmptyToNull() ?? "Naos";
                        document.Info.Description = descriptor?.Tags.ToString(", ").EmptyToNull() ?? "Naos";
                        document.Info.TermsOfService = "None";
                        document.Info.Contact = new NSwag.OpenApiContact
                        {
                            Name = "Naos",
                            Email = string.Empty,
                            Url = "https://github.com/vip32/Naos"
                        };
                    };

                    if (true) // if option.includeSecurityHeader
                    {
                        var provider = sp.GetService<IAuthenticationSchemeProvider>();
                        if (provider?.GetDefaultChallengeSchemeAsync().Result?.Name == AuthenticationKeys.OidcScheme)
                        {
                            c.AddSecurity("Oauth2", new NSwag.OpenApiSecurityScheme
                            {
                                AuthorizationUrl = "https://global-keycloak.azurewebsites.net/auth/realms/master/protocol/openid-connect/auth",
                                Description = "Authorization header using the Oauth2 Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                                Name = "Authorization",
                                Flow = NSwag.OpenApiOAuth2Flow.Implicit,
                                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                                Type = NSwag.OpenApiSecuritySchemeType.OAuth2
                            });
                            c.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Oauth2"));

                            c.AddSecurity("Oidc", new NSwag.OpenApiSecurityScheme
                            {
                                Scheme = "Bearer",
                                OpenIdConnectUrl = "https://global-keycloak.azurewebsites.net/auth/realms/master/.well-known/openid-configuration",
                                //AuthorizationUrl = "https://global-keycloak.azurewebsites.net/auth/realms/master/protocol/openid-connect/auth",
                                Description = "Authorization header using the Oidc Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                                Name = "Authorization",
                                Flow = NSwag.OpenApiOAuth2Flow.Implicit,
                                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                                Type = NSwag.OpenApiSecuritySchemeType.OpenIdConnect // Oauth2/OIDC?,
                            });
                            c.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Oidc"));

                            //c.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
                            //{
                            //    //Schema="Bearer" << has to be added manually in the Auth text box (swaggerui) https://github.com/RicoSuter/NSwag/issues/869
                            //    Description = "Authorization header using a JWT token. Example: \"Authorization: Bearer {jwt}\"",
                            //    Name = "Authorization",
                            //    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                            //    Type = NSwag.OpenApiSecuritySchemeType.ApiKey
                            //});
                            //c.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
                        }
                        else if (provider?.GetDefaultChallengeSchemeAsync().Result?.Name == AuthenticationKeys.ApiKeyScheme)
                        {
                            c.AddSecurity(AuthenticationKeys.ApiKeyScheme, new NSwag.OpenApiSecurityScheme
                            {
                                Description = "Authorization header using the ApiKey scheme. Example: \"Authorization: ApiKey {key}\"",
                                Name = "ApiKey",
                                In = NSwag.OpenApiSecurityApiKeyLocation.Header, // TODO: also allow the auth header to be sent in the querystring
                                Type = NSwag.OpenApiSecuritySchemeType.ApiKey
                            });
                            c.OperationProcessors.Add(new OperationSecurityScopeProcessor(AuthenticationKeys.ApiKeyScheme));
                        }
                        else if (provider?.GetDefaultChallengeSchemeAsync().Result?.Name == AuthenticationKeys.BasicScheme)
                        {
                            c.AddSecurity(AuthenticationKeys.BasicScheme, new NSwag.OpenApiSecurityScheme
                            {
                                Scheme = "Basic",
                                Description = "Authorization header using the Basic scheme. Example: \"Basic: {credentials}\"",
                                Name = "Authorization",
                                In = NSwag.OpenApiSecurityApiKeyLocation.Header, // TODO: also allow the auth header to be sent in the url https://en.wikipedia.org/wiki/Basic_access_authentication
                                Type = NSwag.OpenApiSecuritySchemeType.Basic
                            });
                            c.OperationProcessors.Add(new OperationSecurityScopeProcessor(AuthenticationKeys.BasicScheme));
                        }
                    }
                });

            naosOptions.Context.Messages.Add($"{LogKeys.Startup} naos services builder: swagger documentation added");
            //naosOptions.Context.Services.AddSingleton(new NaosFeatureInformation { Name = "Swagger", Description = "Documentation", EchoRoute = "naos/swagger/echo" });

            return naosOptions;
        }
    }
}

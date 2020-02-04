namespace Naos.Sample.Application.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Application.Web;
    using Naos.Authentication.Application.Web;
    using Naos.Commands.Application;
    using Naos.Commands.Infrastructure.FileStorage;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.JobScheduling.Domain;
    using Naos.Messaging.Domain;
    using Naos.Sample.Catalogs.Application;
    using Naos.Sample.Customers.Application;
    using Naos.Tracing.Domain;
    using Naos.Tracing.Infrastructure;
    using NSwag.AspNetCore;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Security;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true); // https://andrewlock.net/new-in-aspnetcore-3-structured-logging-for-startup-messages/

            //services.AddControllers();

            services
                //.AddMiddlewareAnalysis()
                .AddHttpContextAccessor()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>() // needed for GetUrlHelper (IUrlHelperFactory below)
                .AddScoped(sp =>
                {
                    var actionContext = sp.GetRequiredService<IActionContextAccessor>()?.ActionContext;
                    if (actionContext == null)
                    {
                        throw new ArgumentException("UrlHelper needs an ActionContext, which is usually available in MVC components (Controller/PageModel/ViewComponent)");
                    }

                    var factory = sp.GetRequiredService<IUrlHelperFactory>();
                    return factory?.GetUrlHelper(actionContext);
                })
                .AddMediatr()
                .AddSwaggerDocument((c, sp) => // TODO: replace with .AddOpenApiDocument, but currently has issues with example model generation in UI
                {
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
                })
                .AddMvc(o =>
                {
                    //o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())); // https://tahirnaushad.com/2017/08/28/asp-net-core-2-0-mvc-filters/ or use controller attribute (Authorize)
                })
                    // naos mvc configuration
                    .AddNaos(o =>
                    {
                        // Countries repository is exposed with a dedicated controller, no need to register here
                        o.AddGenericRepositoryController<Customers.Domain.Customer, Customers.Domain.ICustomerRepository>();
                        o.AddGenericRepositoryController<Inventory.Domain.ProductInventory, Inventory.Domain.IInventoryRepository>();
                        o.AddGenericRepositoryController<Inventory.Domain.ProductReplenishment, Inventory.Domain.IReplenishmentRepository>();
                        o.AddGenericRepositoryController<UserAccounts.Domain.UserAccount>(); // =implicit IRepository<UserAccount>
                        o.AddGenericRepositoryController<UserAccounts.Domain.UserVisit>(); // =implicit IRepository<UserVisit>
                    });

            services
                .AddNaos(this.Configuration, "Product", "Capability", new[] { "All" }, n => n
                    //.AddModule<CustomersModule>()
                    .AddModules(m => m
                        .AddCountriesModule()
                        .AddCustomersModule()
                        .AddUserAccountsModule()
                        .AddCatalogsModule()
                        .AddInventoryModule())
                    .AddServiceContext()
                    .AddOidcAuthentication()
                    //.AddAuthenticationApiKeyStatic()
                    //.AddEasyAuthentication(/*o => o.Provider = EasyAuthProviders.AzureActiveDirectory*/)
                    .AddRequestCorrelation()
                    .AddRequestFiltering()
                    .AddServiceExceptions()
                    .AddCommands(o => o
                        .AddBehavior<TracerCommandBehavior>()
                        .AddBehavior<ValidateCommandBehavior>()
                        .AddBehavior<JournalCommandBehavior>()
                        .AddBehavior(sp => new FileStoragePersistCommandBehavior(
                            new FolderFileStorage(o => o
                                .Folder(Path.Combine(Path.GetTempPath(), "naos_commands", "journal")))))
                        .AddEndpoints(o => o
                            .Post<CreateCustomerCommand>(
                                "api/commands/customers/create",
                                onSuccessStatusCode: HttpStatusCode.Created,
                                groupName: "Customers",
                                onSuccess: (cmd, ctx) => ctx.Response.Location($"api/customers/{cmd.Customer.Id}"))
                            .Get<GetActiveCustomersQuery, IEnumerable<Customers.Domain.Customer>>(
                                "api/commands/customers/active",
                                groupName: "Customers")
                            .UseAzureBlobStorage()
                            //.UseInMemoryStorage()
                            //.UseFolderStorage()
                            .UseAzureStorageQueue() // TODO: rabbitmq queue is also needed
                                                    //.UseInMemoryQueue()
                            .GetQueued<PingCommand>("api/commands/queue/ping")
                            .GetQueued<GetActiveCustomersQuery, IEnumerable<Customers.Domain.Customer>>(
                                "api/commands/queue/customers/active",
                                groupName: "Customers")))
                    .AddOperations(o => o
                        .AddInteractiveConsole()
                        .AddLogging(o => o
                            .UseConsole(LogLevel.Debug)
                            .UseFile()
                            //.UseSink(w => w.LiterateConsole())
                            //.UseAzureBlobStorage()
                            //.UseCosmosDb() TODO
                            .UseAzureLogAnalytics(false)
                            .UseMongo())
                        .AddSystemHealthChecks()
                        .AddRequestStorage(o => o
                            .UseAzureBlobStorage())
                        .AddTracing(o => o
                            .UseSampler<ConstantSampler>()
                            .UseZipkinExporter()))
                    //.UseExporter<ZipkinSpanExporter>())) // TODO: UseZipkinExporter + configuration + zipkin url health (options.Endpoint)
                    //.UseSampler(new OperationNamePatternSampler(new[] { "http*" }))))
                    //.AddQueries()
                    //.AddSwaggerDocument() // s.Description = Product.Capability\
                    .AddJobScheduling(o => o
                        //.SetEnabled(true)
                        //.Register<EchoJob>("echojob1", Cron.MinuteInterval(10), (j) => j.EchoAsync("+++ hello from echojob1 +++", CancellationToken.None))
                        //.Register<EchoJob>("manualjob1", Cron.Never(), (j) => j.EchoAsync("+++ hello from manualjob1 +++", CancellationToken.None))
                        .Register<CountriesImportJob>("countriesimport", Cron.MinuteInterval(5)))
                    //.Register("anonymousjob2", Cron.Minutely(), (j) => Console.WriteLine("+++ hello from anonymousjob2 " + j))
                    //.Register("jobevent1", Cron.Minutely(), () => new EchoJobEventData { Text = "+++ hello from jobevent1 +++" }))
                    //.Register<EchoJob>("echojob2", Cron.MinuteInterval(2), j => j.EchoAsync("+++ hello from echojob2 +++", CancellationToken.None, true), enabled: false)
                    //.Register<EchoJob>("testlongjob4", Cron.Minutely(), j => j.EchoLongAsync("+++ hello from testlongjob4 +++", CancellationToken.None)))
                    .AddServiceClient()
                    .AddQueueing()
                    .AddMessaging(o => o
                        //.UseFileStorageBroker(s => s
                        //.UseSignalRServerlessBroker(s => s // WARN: has a bug where old messages are multiplied on new subsequent publishes
                        .UseRabbitMQBroker(s => s
                        //.UseServiceBusBroker(s => s
                           .Subscribe<EchoMessage, EchoMessageHandler>()))
                    .AddServiceDiscovery(o => o
                        .UseFileSystemClientRegistry())
                    // TODO: create a cloud based registry (storage)
                    //.UseConsulClientRegistry())
                    //.UseRouterClientRegistry())
                    .AddServiceDiscoveryRouter(o => o
                        .UseFileSystemRegistry()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseNaos(s => s
                   //.UseAuthenticationChallenge()
                   .UseRequestCorrelation()
                   .UseServiceContext()
                   .UseServicePoweredBy()
                   .UseOperationsHealth()
                   .UseOperationsLogging()
                   .UseOperationsTracing()
                   .UseRequestFiltering()
                   .UseServiceExceptions()
                   .UseCommandRequests()
                   .UseServiceDiscoveryRouter())
               .UseOpenApi()
               .UseSwaggerUi3(a =>
               {
                   a.CustomStylesheetPath = "./css/naos/swagger.css";

                   // Oauth2/Oidc settings
                   a.OAuth2Client = new OAuth2ClientSettings
                   {
                       //AppName = "aspnetcore-keycloak",
                       //Realm = "master",
                       ClientId = "aspnetcore-keycloak", // TODO: get from Configuratoin
                       ClientSecret = "1beb5df9-01dd-46c3-84a8-b65eca50ad57", // TODO: get from Configuratoin
                       // redirect https://localhost:5001/swagger/oauth2-redirect.html
                   };
                   //a.OAuth2Client.AdditionalQueryStringParameters
                    //.AddOrUpdate("response_type", "token") // code?
                    //.AddOrUpdate("scope", "openid profile email claims")
                    //.AddOrUpdate("nonce", "swagger");
                    //.AddOrUpdate("response_mode", "post");
               }); // https://cpratt.co/customizing-swagger-ui-in-asp-net-core/

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseAuthenticationChallenge(); // needs to be last in order, forces login challenge
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

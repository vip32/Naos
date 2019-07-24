namespace Naos.Application.Web
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Web;
    using Naos.Core.Authentication.App.Web;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Commands.Infrastructure.FileStorage;
    using Naos.Core.Configuration.App;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Core.JobScheduling.App;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Core.Messaging.Domain;
    using Naos.Foundation;
    using Newtonsoft.Json;
    using NSwag.AspNetCore;
    using NSwag.Generation.Processors;

    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public Startup(ILogger<Startup> logger)
        {
            this.Configuration = NaosConfigurationFactory.Create();
            this.logger = logger;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMiddlewareAnalysis()
                .AddHttpContextAccessor()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>() // needed for GetUrlHelper (IUrlHelperFactory below)
                .AddScoped(sp =>
                {
                    var actionContext = sp.GetRequiredService<IActionContextAccessor>()?.ActionContext;
                    if(actionContext == null)
                    {
                        throw new ArgumentException("UrlHelper needs an ActionContext, which is usually available in MVC components (Controller/PageModel/ViewComponent)");
                    }

                    var factory = sp.GetRequiredService<IUrlHelperFactory>();
                    return factory?.GetUrlHelper(actionContext);
                })
                .AddSwaggerDocument(config =>
                {
                    config.OperationProcessors.Add(new GenericRepositoryControllerOperationProcessor());
                    config.OperationProcessors.Add(new ApiVersionProcessor());
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "Naos"; // Product.Capability-Version
                        document.Info.Description = "Naos";
                        document.Info.TermsOfService = "None";
                        document.Info.Contact = new NSwag.OpenApiContact
                        {
                            Name = "Naos",
                            Email = string.Empty,
                            Url = "https://github.com/vip32/Naos.Core"
                        };
                    };
                })
                .AddMediatr()
                .AddMvc(o =>
                    {
                        o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())); // https://tahirnaushad.com/2017/08/28/asp-net-core-2-0-mvc-filters/ or use controller attribute (Authorize)
                        //o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().AddRequirements(new[] { new EasyAuthHeadersRequirement("aad") }).Build())); // https://tahirnaushad.com/2017/08/28/asp-net-core-2-0-mvc-filters/ or use controller attribute (Authorize)
                    })
                    // naos mvc configuration
                    .AddNaos(o =>
                    {
                        // Countries repository is exposed with a dedicated controller, no need to register here
                        o.AddGenericRepositoryController<Sample.Customers.Domain.Customer, Sample.Customers.Domain.ICustomerRepository>();
                        o.AddGenericRepositoryController<Sample.UserAccounts.Domain.UserAccount>(); // `=implicit IRepository<UserAccount>
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // naos application services
            services
                .AddNaos(this.Configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddServices(s => s
                        .AddSampleCountries()
                        .AddSampleCustomers()
                        .AddSampleUserAccounts())
                    .AddServiceContext()
                    //.AddAuthenticationApiKeyStatic()
                    .AddEasyAuthentication(/*o => o.Provider = EasyAuthProviders.AzureActiveDirectory*/)
                    .AddRequestCorrelation()
                    .AddRequestFiltering()
                    .AddServiceExceptions()
                    .AddCommands(o => o
                        .AddBehavior<ValidateCommandBehavior>()
                        .AddBehavior<JournalCommandBehavior>()
                        .AddBehavior(new FileStoragePersistCommandBehavior(
                            new FolderFileStorage(o => o
                                .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "commands"))))))
                    .AddOperations(o => o
                        .AddInteractiveConsole()
                        .AddLogging(l => l
                            .UseConsole()
                            .UseFile()
                            //.UseAzureBlobStorage()
                            .UseAzureLogAnalytics())
                        .AddRequestStorage(r => r
                            .UseAzureBlobStorage())
                        .AddTracing())
                    //.AddQueries()
                    //.AddSwaggerDocument() // s.Description = Product.Capability\
                    .AddJobScheduling(o => o
                        //.SetEnabled(true)
                        .Register<EchoJob>("echojob1", Cron.MinuteInterval(5), (j) => j.EchoAsync("+++ hello from echojob1 +++", CancellationToken.None))
                        .Register<EchoJob>("manualjob1", Cron.Never(), (j) => j.EchoAsync("+++ hello from manualjob1 +++", CancellationToken.None)))
                        //.Register("anonymousjob2", Cron.Minutely(), (j) => Console.WriteLine("+++ hello from anonymousjob2 " + j))
                        //.Register("jobevent1", Cron.Minutely(), () => new EchoJobEventData { Text = "+++ hello from jobevent1 +++" }))
                        //.Register<EchoJob>("echojob2", Cron.MinuteInterval(2), j => j.EchoAsync("+++ hello from echojob2 +++", CancellationToken.None, true), enabled: false)
                        //.Register<EchoJob>("testlongjob4", Cron.Minutely(), j => j.EchoLongAsync("+++ hello from testlongjob4 +++", CancellationToken.None)))
                    .AddServiceClient("default")
                    .AddQueueing()
                    .AddMessaging(o => o
                        //.UseFileSystemBroker(s => s
                        //.UseSignalRBroker(s => s
                        //.UseRabbitMQBroker(s => s
                        .UseServiceBusBroker(s => s
                            .Subscribe<EchoMessage, EchoMessageHandler>()))
                    .AddServiceDiscovery(o => o
                        .UseFileSystemClientRegistry())
                    //.UseConsulClientRegistry())
                    //.UseRouterClientRegistry())
                    .AddServiceDiscoveryRouter(o => o
                        .UseFileSystemRegistry()));

            // TODO: need to find a way to start the MessageBroker (done by resolving the IMessageBroker somewhere, HostedService? like scheduling)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment environment)
        {
            if(environment.IsProduction())
            {
                app.UseHsts();
            }

            app
                .UseHttpsRedirection()
                .UseNaos(s => s
                    .UseRequestCorrelation()
                    .UseServiceContext()
                    .UseServicePoweredBy()
                    .UseOperationsLogging()
                    .UseOperationsTracing()
                    .UseRequestFiltering()
                    .UseServiceExceptions()
                    .UseServiceDiscoveryRouter())
                .UseOpenApi()
                .UseSwaggerUi3();

            // https://blog.elmah.io/asp-net-core-2-2-health-checks-explained/
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.UI/ServiceCollectionExtensions.cs
            app.UseHealthChecks("/health", new HealthCheckOptions // TODO: move to UseNaosOperationsHealthChecks
            {
                ResponseWriter = async (c, r) =>
                {
                    c.Response.ContentType = ContentType.JSON.ToValue();
                    await c.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        status = r.Status.ToString(),
                        took = r.TotalDuration.ToString(),
                        checks = r.Entries.Select(e => new
                        {
                            //service = c.GetServiceName(),
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            took = e.Value.Duration.ToString(),
                            message = e.Value.Exception?.Message,
                            data = e.Value.Data
                        })
                    }, DefaultJsonSerializerSettings.Create()));
                }
            });

            //app.UseHealthChecksUI(s =>
            //{
            //    s.ApiPath = "/health/api";
            //    s.UIPath = "/health/ui";
            //});

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}

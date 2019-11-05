namespace Naos.Sample.Application.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
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
    using Naos.Commands.Application;
    using Naos.Commands.Infrastructure.FileStorage;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.JobScheduling.Domain;
    using Naos.Messaging.Domain;
    using Naos.Sample.Catalogs.Application;
    using Naos.Sample.Customers.Application;
    using Naos.Tracing.Domain;
    using NSwag.Generation.Processors;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration; //NaosConfigurationFactory.Create();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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
                    if (true) // option.includeSecurityHeader
                    {
                        c.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme // TODO: dependent on configured authentication
                        {
                            Description = "Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                            Name = "Authorization",
                            In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                            Type = NSwag.OpenApiSecuritySchemeType.ApiKey // Oauth2/OIDC?
                        });
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
                    .AddModules(m => m
                        .AddCountriesModule()
                        .AddCustomersModule()
                        .AddUserAccountsModule()
                        .AddCatalogsModule()
                        .AddInventoryModule())
                    .AddServiceContext()
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
                        .AddRequests(o => o
                            .Post<CreateCustomerCommand>("api/commands/customers/create", HttpStatusCode.Created, "Customers", onSuccess: (cmd, ctx) => ctx.Response.Location($"api/customers/{cmd.Customer.Id}"))
                            .Get<GetActiveCustomersQuery, IEnumerable<Customers.Domain.Customer>>("api/commands/customers/active", groupName: "Customers")
                            //.UseInMemoryQueue()
                            .UseAzureStorageQueue()
                            //.UseInMemoryStorage()
                            //.UseFolderStorage()
                            .UseAzureBlobStorage()
                            .GetQueued<PingCommand>("api/commands/queue/ping")
                            .GetQueued<GetActiveCustomersQuery, IEnumerable<Customers.Domain.Customer>>("api/commands/queue/customers/active", groupName: "Customers")))
                    .AddOperations(o => o
                        .AddInteractiveConsole()
                        .AddLogging(o => o
                            .UseConsole(LogLevel.Debug)
                            .UseFile()
                            //.UseSink(w => w.LiterateConsole())
                            //.UseAzureBlobStorage()
                            .UseAzureLogAnalytics(false)
                            .UseMongo(true))
                        .AddSystemHealthChecks()
                        .AddRequestStorage(o => o
                            .UseAzureBlobStorage())
                        .AddTracing(o => o
                            .UseSampler<ConstantSampler>()))
                    //.UseSampler(new OperationNamePatternSampler(new[] { "http*" }))))
                    //.AddQueries()
                    //.AddSwaggerDocument() // s.Description = Product.Capability\
                    .AddJobScheduling(o => o
                        //.SetEnabled(true)
                        //.Register<EchoJob>("echojob1", Cron.MinuteInterval(10), (j) => j.EchoAsync("+++ hello from echojob1 +++", CancellationToken.None))
                        //.Register<EchoJob>("manualjob1", Cron.Never(), (j) => j.EchoAsync("+++ hello from manualjob1 +++", CancellationToken.None))
                        .Register<CountriesImportJob>("countriesimport", Cron.MinuteInterval(1)))
                    //.Register("anonymousjob2", Cron.Minutely(), (j) => Console.WriteLine("+++ hello from anonymousjob2 " + j))
                    //.Register("jobevent1", Cron.Minutely(), () => new EchoJobEventData { Text = "+++ hello from jobevent1 +++" }))
                    //.Register<EchoJob>("echojob2", Cron.MinuteInterval(2), j => j.EchoAsync("+++ hello from echojob2 +++", CancellationToken.None, true), enabled: false)
                    //.Register<EchoJob>("testlongjob4", Cron.Minutely(), j => j.EchoLongAsync("+++ hello from testlongjob4 +++", CancellationToken.None)))
                    .AddServiceClient()
                    .AddQueueing()
                    .AddMessaging(o => o
                        //.UseFileSystemBroker(s => s
                        //.UseSignalRBroker(s => s
                        //.UseRabbitMQBroker(s => s
                        .UseServiceBusBroker(s => s
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

            app
                .UseNaos(s => s
                    .UseRequestCorrelation()
                    .UseServiceContext()
                    .UseServicePoweredBy()
                    .UseOperationsLogging()
                    .UseOperationsTracing()
                    .UseRequestFiltering()
                    .UseServiceExceptions()
                    .UseCommandRequests()
                    .UseServiceDiscoveryRouter())
                .UseOpenApi()
                .UseSwaggerUi3();

            app.UseRouting();
            //app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

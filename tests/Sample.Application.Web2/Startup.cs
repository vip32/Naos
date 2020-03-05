namespace Naos.Sample.Application.Web
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Application.Web;
    using Naos.Commands.Application;
    using Naos.Commands.Infrastructure.FileStorage;
    using Naos.FileStorage.Infrastructure;
    using Naos.Messaging.Domain;
    using Naos.Queueing.Domain;
    using Naos.Sample.Catalogs.Application;
    using Naos.Sample.Countries.Application;
    using Naos.Sample.Customers.Application;
    using Naos.Sample.Inventory.Application;
    using Naos.Sample.UserAccounts.Application;
    using Naos.Tracing.Domain;
    using NSwag.AspNetCore;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddMvc()
                    .AddNaos(o =>
                    {
                        // Countries repository is exposed with a dedicated controller, no need to register here
                        o.AddEndpoint<Customers.Domain.Customer, Customers.Domain.ICustomerRepository>();
                        o.AddEndpoint<Inventory.Domain.ProductInventory, Inventory.Domain.IInventoryRepository>();
                        o.AddEndpoint<Inventory.Domain.ProductReplenishment, Inventory.Domain.IReplenishmentRepository>();
                        o.AddEndpoint<UserAccounts.Domain.UserAccount>(); // =implicit IRepository<UserAccount>
                        o.AddEndpoint<UserAccounts.Domain.UserVisit>(); // =implicit IRepository<UserVisit>
                    });

            services
                .AddNaos(this.Configuration, "Product", "Capability2", new[] { "All" }, n => n
                    //.AddModule<CustomersModule>()>> INaosModule
                    //.AddModule(m => m { m.Context.Services.AddScoped<....>()}, "customers")
                    //.AddModules() >> discover INaosModule!
                    .AddModules(m => m
                        .AddCountriesModule()
                        .AddCustomersModule()
                        .AddUserAccountsModule()
                        .AddCatalogsModule()
                        .AddInventoryModule())
                    .AddSwaggerDocumentation() // do IMPLICIT! XXXX
                    .AddServiceContext() // do IMPLICIT! XXXX
                    .AddOidcAuthentication()
                    //.AddAuthenticationApiKeyStatic()
                    //.AddEasyAuthentication(/*o => o.Provider = EasyAuthProviders.AzureActiveDirectory*/)
                    .AddRequestCorrelation() // do IMPLICIT!
                    .AddRequestFiltering() // do IMPLICIT! XXXX
                    .AddServiceExceptions() // do IMPLICIT! XXXX
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
                            .UseAzureBlobStorage() // *
                            //.UseInMemoryStorage()
                            //.UseFolderStorage()
                            .UseAzureStorageQueue() // *
                            //.UseAzureServiceBusQueue()
                            //.UseRabbitMQQueue()
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
                            .UseMongo())
                        //.UseSink(w => w.LiterateConsole())
                        //.UseAzureBlobStorage()
                        //.UseCosmosDb() TODO
                        //.UseAzureLogAnalytics())
                        .AddSystemHealthChecks() // do IMPLICIT! XXXXX
                        .AddRequestStorage(o => o
                            .UseAzureBlobStorage())
                        .AddTracing(o => o
                            .UseSampler<ConstantSampler>()
                            .UseZipkinExporter()))
                    //.UseExporter<ZipkinSpanExporter>())) // TODO: UseZipkinExporter + configuration + zipkin url health (options.Endpoint)
                    //.UseSampler(new OperationNamePatternSampler(new[] { "http*" }))))
                    //.AddQueries()
                    //.AddSwaggerDocument() // s.Description = Product.Capability\
                    //.AddJobScheduling(o => o
                        //.SetEnabled(true)
                        //.Register<EchoJob>("echojob1", Cron.MinuteInterval(10), (j) => j.EchoAsync("+++ hello from echojob1 +++", CancellationToken.None))
                        //.Register<EchoJob>("manualjob1", Cron.Never(), (j) => j.EchoAsync("+++ hello from manualjob1 +++", CancellationToken.None))
                        //.Register<CountriesImportJob>("countriesimport", Cron.MinuteInterval(5))
                        //.Register<CountriesExportJob>("countriesexport", Cron.MinuteInterval(2))) // Enqueue
                                                                                                  //.Register("anonymousjob2", Cron.Minutely(), (j) => Console.WriteLine("+++ hello from anonymousjob2 " + j))
                                                                                                  //.Register("jobevent1", Cron.Minutely(), () => new EchoJobEventData { Text = "+++ hello from jobevent1 +++" }))
                                                                                                  //.Register<EchoJob>("echojob2", Cron.MinuteInterval(2), j => j.EchoAsync("+++ hello from echojob2 +++", CancellationToken.None, true), enabled: false)
                                                                                                  //.Register<EchoJob>("testlongjob4", Cron.Minutely(), j => j.EchoLongAsync("+++ hello from testlongjob4 +++", CancellationToken.None)))
                    .AddServiceClient() // do IMPLICIT! XXXXX
                    .AddQueueing(o => o
                        //.UseAzureStorageQueue<EchoQueueEventData>(o => o
                        .UseRabbitMQQueue<EchoQueueEventData>(o => o
                            .ProcessItems())
                        //.UseInMemoryQueue<CountriesExportData>(o => o
                        .UseRabbitMQQueue<CountriesExportData>(o => o
                        //.UseServiceBusQueue<CountriesExportData>(o => o
                            .ProcessItems()))
                    .AddMessaging(o => o
                        //.UseFileStorageBroker(o => o
                        //.UseSignalRServerlessBroker(s => s // WARN: has a bug where old messages are multiplied on new subsequent publishes
                        .UseRabbitMQBroker(o => o
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
                   .UseRequestCorrelation() // do IMPLICIT! XXXXX
                   .UseServiceContext() // do IMPLICIT! XXXXX
                   .UseServicePoweredBy() // do IMPLICIT! XXXXX
                   .UseOperationsHealth() // do IMPLICIT! XXXXX
                   .UseOperationsLogging() // do IMPLICIT! XXXXX
                   .UseOperationsTracing() // do IMPLICIT! XXXXX
                   .UseRequestFiltering() // do IMPLICIT! XXXXX
                   .UseServiceExceptions() // do IMPLICIT! XXXXX
                   .UseCommandEndpoints() // do IMPLICIT! XXXXX
                   .UseServiceDiscoveryRouter())
               .UseOpenApi() // TODO: UseNaos.UseSwaggerDocument()
               .UseSwaggerUi3(a => // TODO: UseNaos.UseSwaggerDocument()
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

namespace Naos.Sample.App.Web
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.Web;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Configuration;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Core.JobScheduling.App;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Operations.App.Web;
    using Naos.Core.RequestCorrelation.App.Web;
    using Naos.Core.ServiceContext.App.Web;
    using Newtonsoft.Json;
    using NSwag.AspNetCore;
    using Polly;
    using Polly.Extensions.Http;

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
            // framework application services
            services.AddTransient<HttpClientLogHandler>();
            services.AddHttpClient("default")
                .AddPolicyHandler((sp, req) =>
                    HttpPolicyExtensions.HandleTransientHttpError()
                        .WaitAndRetryAsync(
                            3,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (outcome, timespan, retryAttempt, context) =>
                            {
                                sp.GetService<ILogger<HttpClient>>()
                                    .LogWarning($"delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}");
                            }))
                .AddPolicyHandler((sp, req) =>
                    HttpPolicyExtensions.HandleTransientHttpError()
                        .CircuitBreakerAsync(
                            3,
                            durationOfBreak: TimeSpan.FromSeconds(30),
                            onBreak: (response, state) =>
                            {
                                sp.GetService<ILogger<HttpClient>>().LogWarning($"break circuit ({state}): {response.Exception.GetFullMessage()}");
                            },
                            onReset: () =>
                            {
                                sp.GetService<ILogger<HttpClient>>().LogInformation("reset circuit");
                            }))
                //.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                //{
                //    TimeSpan.FromSeconds(1),
                //    TimeSpan.FromSeconds(5),
                //    TimeSpan.FromSeconds(10)
                //}))
                //.AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                //    handledEventsAllowedBeforeBreaking: 3,
                //    durationOfBreak: TimeSpan.FromSeconds(30)))
                .AddNaosHttpMessageHandlers();

            services.Replace(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());

            services
                .AddMiddlewareAnalysis()
                .AddHttpContextAccessor()
                .AddSwaggerDocument(s => s.Description = "naos")
                .AddMediatR()
                .AddMvc(o =>
                    {
                        // https://tahirnaushad.com/2017/08/28/asp-net-core-2-0-mvc-filters/ or use controller attribute (Authorize)
                        o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
                    })
                    .AddJsonOptions(o => o.AddDefaultJsonSerializerSettings())
                    .AddControllersAsServices() // https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // naos application services
            services
                .AddNaos(this.Configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddServices(s => s
                        .AddSampleCountries()
                        .AddSampleCustomers()
                        .AddSampleUserAccounts())
                    .AddServiceContext()
                    .AddAuthenticationApiKeyStatic()
                    .AddRequestCorrelation()
                    .AddRequestFiltering()
                    .AddServiceExceptions()
                    .AddCommands()
                    .AddOperations(o => o
                        //.AddRequestFileStorage(r => r.UseAzureBlobStorage())
                        .AddLogging(l => l
                            .UseFile()
                            .UseAzureBlobStorage()
                            .UseAzureLogAnalytics()))
                    //.AddQueries()
                    //.AddSwaggerDocument() // s.Description = Product.Capability\
                    .AddJobScheduling(o => o
                        .SetEnabled(true)
                        .Register<DummyJob>("job1", Cron.Minutely(), (j) => j.LogMessageAsync("+++ hello from job1 +++", CancellationToken.None))
                        .Register<DummyJob>("job2", Cron.MinuteInterval(2), j => j.LogMessageAsync("+++ hello from job2 +++", CancellationToken.None, true), enabled: false)
                        .Register<DummyJob>("longjob33", Cron.Minutely(), j => j.LongRunningAsync("+++ hello from longjob3 +++", CancellationToken.None)))
                    .AddMessaging(o => o
                        //.UseFileSystemBroker(s => s
                        //.UseSignalRBroker(s => s
                        //.UseRabbitMQBroker(s => s
                        .UseServiceBusBroker(s => s
                            .Subscribe<TestMessage, TestMessageHandler>()))
                    .AddServiceDiscovery(o => o
                        .UseFileSystemClientRegistry()));
            //.UseConsulClientRegistry()));
            //.UseFileSystemRouterRegistry()));
            //.UseRemoteRouterClientRegistry()));

            // TODO: need to find a way to start the MessageBroker (done by resolving the IMessageBroker somewhere, HostedService? like scheduling)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, DiagnosticListener diagnosticListener, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            this.logger.LogInformation($"app {env.ApplicationName} environment: {env.EnvironmentName}");
            //diagnosticListener.SubscribeWithAdapter(new NaosDiagnosticListener());

            if (env.IsProduction())
            {
                app.UseHsts();
            }

            // "002c1126-c7b6-4d8a-ba87-4a997d8b0c6f"

            // naos middleware
            app
                .UseHttpsRedirection() // TODO: UseNaos()...... with setupAction like services
                .UseNaos()
                .UseNaosRequestCorrelation()
                .UseNaosServiceContext()
                .UseNaosServicePoweredBy()
                .UseNaosOperations(
                    new OperationsLoggingOptions // DI ABOVE ^^ (configure)
                    {
                        // following for RequestFileStorageMiddleware > bundle in context (configure above) + di register.
                        //                                              this context is then injected into middleware
                        RequestFileStorage = new FileStorageScopedDecorator("requests/{yyyy}/{MM}/{dd}",
                            new FileStorageLoggingDecorator(
                                app.ApplicationServices.GetRequiredService<ILoggerFactory>(),
                                //new FolderFileStorage(f => f.Folder(Path.Combine(Path.GetTempPath(), "naos_operations")))))
                                new AzureBlobFileStorage(f => f
                                    .ContainerName($"{env.EnvironmentName.ToLower()}-operations")
                                    .ConnectionString(this.Configuration["naos:operations:logging:azureBlobStorage:connectionString"]))))
                    })
                .UseNaosRequestFiltering()
                .UseNaosExceptionHandling()
                .UseNaosServiceDiscoveryRouter();

            app.UseSwagger();
            app.UseSwaggerUi3();

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
                            service = c.GetServiceName(),
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            took = e.Value.Duration.ToString(),
                            message = e.Value.Exception?.Message
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

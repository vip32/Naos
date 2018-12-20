namespace Naos.Sample.App.Web
{
    using System;
    using System.Linq;
    using System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Correlation.Web;
    using Naos.Core.App.Web;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Correlation.Web;
    using Naos.Core.Scheduling.App;
    using Naos.Core.Scheduling.Domain;
    using Newtonsoft.Json;
    using NSwag.AspNetCore;

    public class Startup
    {
        private readonly ILogger<Startup> logger;

        public Startup(ILogger<Startup> logger)
        {
            this.Configuration = NaosConfigurationFactory.CreateRoot();
            this.logger = logger;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<HttpClientCorrelationHandler>();
            services.AddTransient<HttpClientLogHandler>();

            services.AddHttpClient("default")
                .AddHttpMessageHandler<HttpClientCorrelationHandler>();
                //.AddHttpMessageHandler<HttpClientLogHandler>();

            //services.AddSingleton(sp => new HttpClient(
            //    new HttpMessageHandlerBuilder(sp.GetRequiredService<ILogger>())
            //        .Add(new HttpClientCorrelationHandler()).Build()));
            // or https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
            //services.Replace(ServiceDescriptor.Singleton<Microsoft.Extensions.Http.IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());

            services
                .AddHttpContextAccessor()
                .AddHealthChecksUI()
                .AddSwaggerDocument(s => s.Description = "naos")
                .AddMediatR() // singleton due to inmemory repository: c => c.AsSingleton()
                .AddMvc()
                    .AddJsonOptions(o => o.AddDefaultJsonSerializerSettings())
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // naos application services
            services
                .AddNaosCorrelation()
                .AddNaosOperationsSerilog(this.Configuration)
                .AddNaosOperationsLogAnalytics(this.Configuration)
                .AddNaosExceptionHandling(/*env.IsProduction()*/)
                .AddNaosScheduling(s => s
                    .SetEnabled(false)
                    .Register<DummyJob>("job1", Cron.Minutely(), (j) => j.LogMessageAsync("+++ hello from job1 +++", CancellationToken.None))
                    .Register<DummyJob>("job2", Cron.MinuteInterval(2), j => j.LogMessageAsync("+++ hello from job2 +++", CancellationToken.None, true), enabled: false)
                    .Register<DummyJob>("longjob3", Cron.Minutely(), j => j.LongRunningAsync("+++ hello from longjob3 +++", CancellationToken.None)))
                .AddNaosMessagingServiceBus(this.Configuration, AppDomain.CurrentDomain.FriendlyName)
                .AddNaosAppCommands();

            // naos sample product registrations
            services
                .AddSampleCountries()
                .AddSampleCustomers(this.Configuration)
                .AddSampleUserAccounts(this.Configuration);

            //this.IntegrateSimpleInjector(services, this.container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app/*, IServiceCollection services*/, IHostingEnvironment env)
        {
            //this.ConfigureContainer(app, env);

            this.logger.LogInformation($"app {env.ApplicationName} environment: {env.EnvironmentName}");
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            // Middleware
            app.UseHttpsRedirection();
            app.UseNaosCorrelation(); // TODO: convert to proper IMiddleware (to get the injection working), like UseNaosExceptionHandling
            app.UseNaosExceptionHandling();

            app.UseSwagger();
            app.UseSwaggerUi3();

            // https://blog.elmah.io/asp-net-core-2-2-health-checks-explained/
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.UI/ServiceCollectionExtensions.cs
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (c, r) =>
                {
                    c.Response.ContentType = ContentType.JSON.ToValue();
                    var result = JsonConvert.SerializeObject(new
                    {
                        status = r.Status.ToString(),
                        took = r.TotalDuration.ToString(),
                        checks = r.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), took = e.Value.Duration.ToString(), message = e.Value.Exception?.Message })
                    }, DefaultJsonSerializerSettings.Create());
                    await c.Response.WriteAsync(result);
                }
            });

            app.UseHealthChecksUI(s =>
            {
                s.ApiPath = "/health/api";
                s.UIPath = "/health/ui";
            });

            app.UseMvc();
        }
    }
}

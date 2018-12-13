namespace Naos.Sample.App.Web
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Correlation.Web;
    using Naos.Core.App.Web;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Scheduling.App;
    using Naos.Core.Scheduling.Domain;
    using Newtonsoft.Json;
    using NSwag.AspNetCore;
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCore.Mvc;
    using SimpleInjector.Lifestyles;

    public class Startup
    {
        private readonly ILogger<Startup> logger;
        private readonly Container container = new Container();

        public Startup(ILogger<Startup> logger)
        {
            this.Configuration = NaosConfigurationFactory.CreateRoot();
            this.logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddHealthChecksUI()
                .AddSwaggerDocument(s => s.Description = "naos")
                .AddMvc()
                    .AddJsonOptions(o => o.AddDefaultJsonSerializerSettings())
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // naos application services
            services
                .AddNaosCorrelation()
                .AddNaosLoggingSerilog(this.Configuration)
                .AddNaosOperationsLogAnalytics(this.Configuration)
                .AddNaosExceptionHandling(/*env.IsProduction()*/)
                .AddNaosScheduling(s => s
                    .Register<DummyJob>("job1", Cron.Minutely(), (j) => j.LogMessageAsync("+++ hello from job1 +++", CancellationToken.None))
                    .Register<DummyJob>("job2", Cron.MinuteInterval(2), j => j.LogMessageAsync("+++ hello from job2 +++", CancellationToken.None, true))
                    .Register<DummyJob>("longjob3", Cron.Minutely(), j => j.LongRunningAsync("+++ hello from longjob3 +++", CancellationToken.None)))
                .AddNaosMessaging(this.Configuration, AppDomain.CurrentDomain.FriendlyName);

            this.container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            this.container
                // https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/issues/12
                // https://ardalis.com/using-mediatr-in-aspnet-core-apps
                .AddNaosMediator(new[] { typeof(IEntity).Assembly, typeof(Customers.Domain.Customer).Assembly })
                .AddNaosAppCommands(new[] { typeof(Customers.Domain.Customer).Assembly });

            // naos sample product registrations
            services
                .AddSampleCountries()
                .AddSampleCustomers(this.Configuration)
                .AddSampleUserAccounts(this.Configuration);

            this.IntegrateSimpleInjector(services, this.container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app/*, IServiceCollection services*/, IHostingEnvironment env)
        {
            this.ConfigureContainer(app, env);

            this.logger.LogInformation($"app {env.ApplicationName} environment: {env.EnvironmentName}");
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            // Middleware
            app.UseHttpsRedirection();
            app.UseNaosCorrelation(); // TODO: convert to proper IMiddleware (to get the injection working), like UseNaosExceptionHandling
            app.UseNaosExceptionHandling(this.container);

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

        private void IntegrateSimpleInjector(IServiceCollection services, Container container) // TODO: move to App.Web (extension method)
        {
            //container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(this.container));
            services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(this.container));
            services.EnableSimpleInjectorCrossWiring(this.container);
            services.UseSimpleInjectorAspNetRequestScoping(this.container);
        }

        private void ConfigureContainer(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Add application presentation components:
            this.container.RegisterMvcControllers(app);
            this.container.RegisterMvcViewComponents(app);
            this.container.AutoCrossWireAspNetComponents(app);
        }
    }
}

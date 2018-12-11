namespace Naos.Sample.App.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Commands;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Correlation.Web;
    using Naos.Core.App.Exceptions.Web;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.App.Web;
    using Naos.Core.Common.Dependency.SimpleInjector;
    using Naos.Core.Domain;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;
    using Naos.Core.Scheduling;
    using Naos.Core.Scheduling.App;
    using Naos.Core.Scheduling.Domain;
    using Naos.Sample.Countries;
    using Naos.Sample.Customers;
    using Naos.Sample.UserAccounts;
    using Naos.Sample.UserAccounts.EntityFramework;
    using NSwag.AspNetCore;
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCore.Mvc;
    using SimpleInjector.Lifestyles;

    public class Startup
    {
        private static readonly Random Random = new Random(DateTime.Now.GetHashCode());
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
                .AddSwaggerDocument()
                .AddNaosCorrelation()
                .AddMvc().AddJsonOptions(o => o.AddDefaultJsonSerializerSettings())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Naos application services.
            this.container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            this.container
                .AddNaosMediator(new[] { typeof(IEntity).Assembly, typeof(Customers.Domain.Customer).Assembly })
                .AddNaosLogging(this.Configuration)
                //.AddNaosCorrelation() // TODO: use this instead of services.AddNaosCorrelation()
                .AddNaosExceptionHandling(services, false/*env.IsProduction()*/)
                .AddNaosOperations(this.Configuration)
                .AddNaosAppCommands(new[] { typeof(Customers.Domain.Customer).Assembly })
                .AddNaosMessaging(
                    this.Configuration,
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBroker).Assembly, typeof(Customers.Domain.Customer).Assembly })
                .AddNaosScheduling(services); // TODO: register jobs here somehow

            // naos sample product registrations
            this.container
                .AddSampleCountries()
                .AddSampleCustomers(this.Configuration)
                .AddSampleUserAccounts(this.Configuration, services);
            //.Verify(); // TODO: move to health check?

            this.IntegrateSimpleInjector(services, this.container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app/*, IServiceCollection services*/, IHostingEnvironment env)
        {
            this.ConfigureContainer(app, env);
            //this.container.Verify();

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
            //app.UseHealthChecks("/health");
            //app.UseHealthChecksUI(/*s => s.ApiPath = "/health/ui"*/);

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
            // Allow Simple Injector to resolve services from ASP.NET Core.
            this.container.AutoCrossWireAspNetComponents(app);

            this.InitializeSchedular(this.container.GetService<IJobScheduler>());
        }

        private void InitializeSchedular(IJobScheduler scheduler) => scheduler // TODO: where to put these registrations
                .Register<DummyJob>("job1", Cron.Minutely(), (j) => j.LogMessageAsync("+++ hello from job1 +++", CancellationToken.None))
                .Register<DummyJob>("job2", Cron.MinuteInterval(2), j => j.LogMessageAsync("+++ hello from job2 +++", CancellationToken.None, true))
                .Register<DummyJob>("longjob3", Cron.Minutely(), j => j.LongRunningAsync("+++ hello from longjob3 +++", CancellationToken.None));
                //.Register("longjob4", Cron.Minutely(), async (a) =>
                //{
                //    for (int i = 1; i <= 5; i++)
                //    {
                //        System.Diagnostics.Trace.WriteLine($"+++ hello from LONG task #{i}");
                //        await Task.Delay(new TimeSpan(0, 0, 45));
                //    }
                //});
    }
}

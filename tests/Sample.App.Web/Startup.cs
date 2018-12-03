namespace Naos.Sample.App.Web
{
    using System;
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
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCore.Mvc;
    using SimpleInjector.Lifestyles;

    public class Startup
    {
        private static readonly Random Random = new Random(DateTime.Now.GetHashCode());
        private readonly Container container = new Container();

        public Startup()
        {
            this.Configuration = NaosConfigurationFactory.CreateRoot();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddNaosCorrelation()
                .AddDbContext<UserAccountsContext>(options => options.UseNaosSqlServer(this.Configuration)) // needed for migrations:add/update
                .AddMvc(o =>
                {
                    //o.Filters.Add(typeof(GlobalExceptionFilter)); TODO
                }).AddJsonOptions(o => o.AddDefaultJsonSerializerSettings())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            this.IntegrateSimpleInjector(services, this.container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            this.InitializeContainer(app);
            //this.container.Verify();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseNaosCorrelation();
            app.UseMvc();
        }

        private void IntegrateSimpleInjector(IServiceCollection services, Container container) // TODO: move to App.Web (extension method)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(this.container));
            services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(this.container));
            services.EnableSimpleInjectorCrossWiring(this.container);
            services.UseSimpleInjectorAspNetRequestScoping(this.container);
            // TODO: temporary solution to get the scheduler hosted service to run (with its dependencies)
            // https://stackoverflow.com/questions/50394666/injecting-simple-injector-components-into-ihostedservice-with-asp-net-core-2-0#
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
                new JobSchedulerHostedService(sp.GetService<ILogger<JobSchedulerHostedService>>(), container));
        }

        private void InitializeContainer(IApplicationBuilder app)
        {
            // Add application presentation components:
            this.container.RegisterMvcControllers(app);
            this.container.RegisterMvcViewComponents(app);

            // Naos application services.
            this.container
                .AddNaosMediator(new[] { typeof(IEntity).Assembly, typeof(Customers.Domain.Customer).Assembly })
                .AddNaosLogging(this.Configuration)
                .AddNaosOperations(this.Configuration)
                .AddNaosAppCommands(new[] { typeof(Customers.Domain.Customer).Assembly })
                .AddNaosMessaging(
                    this.Configuration,
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBroker).Assembly, typeof(Customers.Domain.Customer).Assembly })
                .AddNaosScheduling();

            // naos sample registrations
            this.container
                .AddSampleCountries()
                .AddSampleCustomers(this.Configuration)
                .AddSampleUserAccounts();

            // Allow Simple Injector to resolve services from ASP.NET Core.
            this.container.AutoCrossWireAspNetComponents(app);

            this.InitializeSchedular(this.container.GetService<IJobScheduler>());
        }

        private void InitializeSchedular(IJobScheduler scheduler)
        {
            scheduler
                .Register("key1", Cron.Minutely(), (a) => System.Diagnostics.Trace.WriteLine("+++ hello from task1 +++"))
                .Register(Cron.MinuteInterval(2), (a) =>
                {
                    System.Diagnostics.Trace.WriteLine("+++ hello from task2 +++");

                    if (Random.Next(2) == 0)// throw randomly
                    {
                        throw new ApplicationException("+++ ohoh error from task 2");
                    }
                })
                .Register("long1", Cron.Minutely(), async (a) =>
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        System.Diagnostics.Trace.WriteLine($"+++ hello from LONG task #{i}");
                        await Task.Delay(new TimeSpan(0, 0, 45));
                    }
                });
        }
    }
}

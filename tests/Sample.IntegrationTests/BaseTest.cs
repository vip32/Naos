namespace Naos.Sample.IntegrationTests
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.App.Configuration;
    using Naos.Core.Domain;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Core.Messaging;
    using Naos.Sample.Customers.Domain;
    using Naos.Sample.UserAccounts.EntityFramework;
    using SimpleInjector;

    public abstract class BaseTest
    {
        protected readonly Container container = new Container();
        protected readonly IServiceCollection services = new ServiceCollection();

        protected BaseTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ASPNETCORE_ISLOCAL", "True");

            var configuration = NaosConfigurationFactory.CreateRoot();

            // naos core registrations
            this.services
                .AddNaosLoggingSerilog(configuration)
                .AddNaosOperationsLogAnalytics(configuration)
                .AddNaosMessaging(configuration, AppDomain.CurrentDomain.FriendlyName);

            this.container
                .AddNaosMediator(new[] { typeof(IEntity).Assembly, typeof(BaseTest).Assembly, typeof(Customer).Assembly })
                .AddNaosAppCommands(new[] { typeof(Customer).Assembly });

            // naos sample registrations
            this.services
                .AddSampleCountries()
                .AddSampleCustomers(configuration)
                .AddSampleUserAccounts(configuration, null, dbContext: new UserAccountsContext(new DbContextOptionsBuilder().UseNaosSqlServer(configuration, "naos:sample:userAccounts:entityFramework").Options));

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
namespace Naos.Sample.IntegrationTests
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.App.Commands;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Common.Dependency.SimpleInjector;
    using Naos.Core.Domain;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;
    using Naos.Sample.Countries;
    using Naos.Sample.Customers;
    using Naos.Sample.Customers.Domain;
    using Naos.Sample.UserAccounts;
    using Naos.Sample.UserAccounts.EntityFramework;
    using SimpleInjector;

    public abstract class BaseTest
    {
        protected readonly Container container = new Container();

        protected BaseTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ASPNETCORE_ISLOCAL", "True");

            var configuration = NaosConfigurationFactory.CreateRoot();

            // naos core registrations
            this.container
                .AddNaosMediator(new[] { typeof(IEntity).Assembly, typeof(BaseTest).Assembly, typeof(Customer).Assembly })
                .AddNaosLogging(configuration)
                .AddNaosAppCommands(new[] { typeof(Customer).Assembly })
                .AddNaosOperations(configuration)
                .AddNaosMessaging(
                    configuration,
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBroker).Assembly, typeof(BaseTest).Assembly, typeof(Customer).Assembly });

            // naos sample registrations
            this.container
                .AddSampleCountries()
                .AddSampleCustomers(configuration)
                .AddSampleUserAccounts(new UserAccountsContext(new DbContextOptionsBuilder().UseNaosSqlServer(configuration).Options));

            //this.container.Verify();
        }
    }
}
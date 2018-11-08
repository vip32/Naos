namespace Naos.Sample.IntegrationTests
{
    using System;
    using Naos.Core.App.Commands;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Common.Dependency.SimpleInjector;
    using Naos.Core.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Naos.Sample.Customers;
    using Naos.Sample.Customers.Domain;
    using Naos.Sample.UserAccounts;
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
                .AddNaosLogging()
                .AddNaosAppCommands(new[] { typeof(Customer).Assembly })
                .AddNaosMessaging(
                    configuration,
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBus).Assembly, typeof(BaseTest).Assembly, typeof(Customer).Assembly });

            // naos sample registrations
            this.container
                .AddSampleCustomers(configuration)
                .AddSampleUserAccounts(configuration);

            this.container.Verify();
        }
    }
}
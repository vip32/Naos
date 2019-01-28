namespace Naos.Sample.IntegrationTests
{
    using System;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Commands.App;
    using Naos.Core.Common;
    using Naos.Core.Configuration;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.EntityFramework;

    public abstract class BaseTest
    {
        protected readonly IServiceCollection services = new ServiceCollection();

        protected BaseTest()
        {
            Environment.SetEnvironmentVariable(EnvironmentKeys.Environment, "Development");
            Environment.SetEnvironmentVariable(EnvironmentKeys.IsLocal, "True");

            var configuration = NaosConfigurationFactory.Create();

            // naos core registrations
            this.services
                .AddMediatR()
                .AddNaosOperationsSerilog(configuration, correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}")
                .AddNaosOperationsLogAnalytics(configuration)
                //.AddNaosMessagingFileSystem(configuration)
                .AddNaosMessagingAzureServiceBus(configuration)
                .AddNaosAppCommands();

            // naos sample registrations
            this.services
                .AddSampleCountries()
                .AddSampleCustomers(configuration)
                .AddSampleUserAccounts(configuration, dbContext: new UserAccountsContext(new DbContextOptionsBuilder().UseNaosSqlServer(configuration, "naos:sample:userAccounts:entityFramework").Options));

            this.services.AddSingleton<ICommandBehavior, TrackCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>();
            this.services.AddSingleton<ICommandBehavior, IdempotentCommandBehavior>();
            this.services.AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; }
    }
}
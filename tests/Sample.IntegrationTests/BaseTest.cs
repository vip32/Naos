namespace Naos.Sample.IntegrationTests
{
    using System;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Commands.Domain;
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
                .AddNaos(configuration, "Product", "Capability")
                    .AddServices(o => o
                        .AddSampleCountries()
                        .AddSampleCustomers()
                        .AddSampleUserAccounts(dbContext: new UserAccountsContext(new DbContextOptionsBuilder().UseNaosSqlServer(configuration, "naos:sample:userAccounts:entityFramework").Options)))
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}"))
                    .AddMessaging(o => o
                        //.AddFileSystemBroker()
                        //.AddSignalRBroker()
                        .AddServiceBusBroker())
                    .AddCommands();

            this.services.AddSingleton<ICommandBehavior, TrackCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>();
            this.services.AddSingleton<ICommandBehavior, IdempotentCommandBehavior>();
            this.services.AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; }
    }
}
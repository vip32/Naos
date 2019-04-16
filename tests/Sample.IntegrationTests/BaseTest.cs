namespace Naos.Sample.IntegrationTests
{
    using System;
    using System.IO;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Commands.Infrastructure.FileStorage;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Infrastructure.EntityFramework;

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
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddServices(o => o
                        .AddSampleCountries()
                        .AddSampleCustomers()
                        .AddSampleUserAccounts(
                            dbContext: new UserAccountsContext(new DbContextOptionsBuilder().UseNaosSqlServer(configuration, "naos:sample:userAccounts:entityFramework").Options)))
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{IdGenerator.Instance.Next}"))
                    .AddMessaging(o => o
                        //.AddFileSystemBroker()
                        //.AddSignalRBroker()
                        .UseServiceBusBroker())
                    .AddCommands());

            this.services.AddSingleton<ICommandBehavior, ValidateCommandBehavior>(); // new ValidateCommandBehavior(false)
            this.services.AddSingleton<ICommandBehavior, TrackCommandBehavior>();
            this.services.AddSingleton<ICommandBehavior>(new FileStoragePersistCommandBehavior(
                            new FolderFileStorage(o => o
                                .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "commands")))));
            //this.services.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, IdempotentCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; }
    }
}
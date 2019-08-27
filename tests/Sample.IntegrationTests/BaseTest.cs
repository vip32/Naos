namespace Naos.Sample.IntegrationTests
{
    using System;
    using System.IO;
    using System.Linq;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Commands.Infrastructure.FileStorage;
    using Naos.Core.Configuration.App;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.UserAccounts.Infrastructure.EntityFramework;

    public abstract class BaseTest
    {
        protected BaseTest()
        {
            Environment.SetEnvironmentVariable(EnvironmentKeys.Environment, "Development");
            Environment.SetEnvironmentVariable(EnvironmentKeys.IsLocal, "True");

            var configuration = NaosConfigurationFactory.Create();
            var entityFrameworkConfiguration = configuration.GetSection("naos:sample:userAccounts:entityFramework").Get<EntityFrameworkConfiguration>();

            // naos core registrations
            this.Services
                .AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GetName().Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)).ToArray())
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddServices(o => o
                        .AddSampleCountries()
                        .AddSampleCustomers()
                        .AddSampleUserAccounts($"Server=(localdb)\\mssqllocaldb;Database={nameof(UserAccountsDbContext)};Trusted_Connection=True;MultipleActiveResultSets=True;"))
                    //.AddSampleUserAccounts(dbContext: new UserAccountsContext(
                    //    new DbContextOptionsBuilder()
                    //        .UseSqlServer(entityFrameworkConfiguration.ConnectionString)
                    //        .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    //        .EnableDetailedErrors().Options))) // "Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=True;MultipleActiveResultSets=True;"
                    //dbContext: new UserAccountsContext(new DbContextOptionsBuilder().UseNaosSqlServer(configuration, "naos:sample:userAccounts:entityFramework").Options)))
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{IdGenerator.Instance.Next}")
                        .AddTracing())
                    .AddMessaging(o => o
                        //.AddFileSystemBroker()
                        //.AddSignalRBroker()
                        .UseServiceBusBroker())
                    .AddCommands());

            this.Services.AddSingleton<ICommandBehavior, ValidateCommandBehavior>(); // new ValidateCommandBehavior(false)
            this.Services.AddSingleton<ICommandBehavior, JournalCommandBehavior>();
            this.Services.AddSingleton<ICommandBehavior>(new FileStoragePersistCommandBehavior(
                            new FolderFileStorage(o => o
                                .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "commands")))));
            //this.services.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, IdempotentCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            this.ServiceProvider = this.Services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; }

        protected IServiceCollection Services { get; } = new ServiceCollection();
    }
}
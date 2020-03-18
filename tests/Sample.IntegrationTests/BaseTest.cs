namespace Naos.Sample.IntegrationTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Commands.Application;
    using Naos.Commands.Infrastructure.FileStorage;
    using Naos.Configuration.Application;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Catalogs.Application;
    using Naos.Sample.Customers.Application;
    using Naos.Sample.Inventory.Application;
    using Naos.Sample.UserAccounts.Application;
    using Naos.Sample.UserAccounts.Infrastructure;
    using Xunit.Abstractions;

    public abstract class BaseTest
    {
        private static IConfigurationRoot configuration;

        protected BaseTest()
        {
            Environment.SetEnvironmentVariable(EnvironmentKeys.Environment, "Development");
            Environment.SetEnvironmentVariable(EnvironmentKeys.IsLocal, "True");

            var entityFrameworkConfiguration = Configuration.GetSection("naos:sample:userAccounts:entityFramework").Get<EntityFrameworkConfiguration>();

            // naos core registrations
            this.Services
                .AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GetName().Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)).ToArray())
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddModules(o => o
                        .AddCountriesModule()
                        .AddCustomersModule()
                        .AddUserAccountsModule($"Server=(localdb)\\mssqllocaldb;Database={nameof(UserAccountsDbContext)};Trusted_Connection=True;MultipleActiveResultSets=True;")
                        .AddCatalogsModule()
                        .AddInventoryModule())
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

            this.Services.AddSingleton<ICommandBehavior>(new ValidateCommandBehavior(false));
            this.Services.AddSingleton<ICommandBehavior, JournalCommandBehavior>();
            this.Services.AddSingleton<ICommandBehavior>(new FileStoragePersistCommandBehavior(
                            new FolderFileStorage(o => o
                                .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "commands")))));
            //this.services.AddSingleton<ICommandBehavior, ServiceContextEnrichCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, IdempotentCommandBehavior>();
            //this.services.AddSingleton<ICommandBehavior, PersistCommandBehavior>();

            this.ServiceProvider = this.Services.BuildServiceProvider();
        }

        protected static IConfiguration Configuration
        {
            get
            {
                return configuration ?? (configuration = NaosConfigurationFactory.Create());
            }
        }

        protected ServiceProvider ServiceProvider { get; }

        protected IServiceCollection Services { get; } = new ServiceCollection();

        protected long Benchmark(Action action, int iterations = 1, ITestOutputHelper output = null)
        {
            GC.Collect();
            var sw = new Stopwatch();
            action(); // trigger jit before execution

            sw.Start();
            for (var i = 1; i <= iterations; i++)
            {
                action();
            }

            sw.Stop();
            output?.WriteLine($"Execution with #{iterations} iterations took: {sw.Elapsed.TotalMilliseconds}ms\r\n  - Gen-0: {GC.CollectionCount(0)}, Gen-1: {GC.CollectionCount(1)}, Gen-2: {GC.CollectionCount(2)}", sw.ElapsedMilliseconds);

            return sw.ElapsedMilliseconds;
        }
    }
}
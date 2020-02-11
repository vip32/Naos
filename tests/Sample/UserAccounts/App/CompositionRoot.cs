namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Foundation.Application.Web.Startup.EntityFramework;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.Infrastructure;
    using Naos.Tracing.Domain;

    /// <summary>
    /// Naos service extensions.
    /// </summary>
    public static partial class CompositionRoot
    {
        public static ModuleOptions AddUserAccountsModule(
            this ModuleOptions options,
            string connectionString = null,
            string section = "naos:sample:userAccounts:entityFramework",
            UserAccountsDbContext dbContext = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("useraccounts");

            if (dbContext != null)
            {
                options.Context.Services.AddSingleton(dbContext); // cross wiring, warning this will be a singleton (not scoped)
            }

            var configuration = options.Context.Configuration?.GetSection(section).Get<EntityFrameworkConfiguration>();

            options.Context.Services.AddScoped<IGenericRepository<UserAccount>>(sp =>
            {
                return new UserAccountRepository(
                    new RepositoryTracingDecorator<UserAccount>(
                        sp.GetService<ILogger<UserAccountRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<UserAccount>(
                            sp.GetRequiredService<ILogger<UserAccountRepository>>(),
                            new RepositoryTenantDecorator<UserAccount>(
                                "naos_sample_test", // TODO: resolve from runtime context
                                new RepositorySoftDeleteDecorator<UserAccount>(
                                    new EntityFrameworkRepository<UserAccount>(o => o
                                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                        .Mediator(sp.GetRequiredService<IMediator>())
                                        .DbContext(sp.GetRequiredService<UserAccountsDbContext>())))))));
            });

            options.Context.Services.AddScoped<IGenericRepository<UserVisit>>(sp =>
            {
                return new UserVisitRepository(
                    new RepositoryTracingDecorator<UserVisit>(
                        sp.GetService<ILogger<UserVisitRepository>>(),
                        sp.GetService<ITracer>(),
                        new RepositoryLoggingDecorator<UserVisit>(
                            sp.GetRequiredService<ILogger<UserVisitRepository>>(),
                            new RepositoryTenantDecorator<UserVisit>(
                                "naos_sample_test", // TODO: resolve from runtime context
                                new RepositorySoftDeleteDecorator<UserVisit>(
                                    new EntityFrameworkRepository<UserVisit>(o => o
                                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                        .Mediator(sp.GetRequiredService<IMediator>())
                                        .DbContext(sp.GetRequiredService<UserAccountsDbContext>())
                                        .Mapper(new AutoMapperEntityMapper(MapperFactory.Create()))))))));
            });

            options.Context.Services.AddDbContext<UserAccountsDbContext>(o => o
                .UseSqlServer("Server=127.0.0.1;Database=naos_sample;User=sa;Password=Abcd1234!;Trusted_Connection=False;MultipleActiveResultSets=True;", o => o // docker
                //.UseSqlServer(connectionString.EmptyToNull() ?? configuration.ConnectionString.EmptyToNull() ?? $"Server=(localdb)\\mssqllocaldb;Database={nameof(UserAccountsDbContext)};Trusted_Connection=True;MultipleActiveResultSets=True;", o => o
                    .MigrationsHistoryTable("__MigrationsHistory", "useraccounts")
                    .EnableRetryOnFailure())
                .UseLoggerFactory(options.Context.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>())
                //.ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

            options.Context.Services.AddStartupTask<ApplyPendingMigrationsTask<UserAccountsDbContext>>();
            options.Context.Services.AddStartupTask<EchoStartupTask>(new TimeSpan(0, 0, 3));

            options.Context.Services.AddHealthChecks()
                .AddSqlServer(configuration.ConnectionString, name: "UserAccounts-sqlserver");

            options.Context.Services.AddHealthChecks() // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1#entity-framework-core-dbcontext-probe
                .AddDbContextCheck<UserAccountsDbContext>(name: "UserAccounts-dbcontext");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: useraccounts service added");

            return options;
        }
    }
}

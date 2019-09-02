namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.App.Web;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.Infrastructure.EntityFramework;

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

            options.Context.AddTag("UserAccounts");

            if (dbContext != null)
            {
                options.Context.Services.AddSingleton(dbContext); // cross wiring, warning this will be a singleton (not scoped)
            }

            options.Context.Services.AddScoped<IGenericRepository<UserAccount>>(sp =>
            {
                return new UserAccountRepository(
                    new RepositoryTracingDecorator<UserAccount>(
                        sp.GetRequiredService<ITracer>(),
                        sp.GetRequiredService<ILogger<UserAccountRepository>>(),
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

            var entityFrameworkConfiguration = options.Context.Configuration?.GetSection(section).Get<EntityFrameworkConfiguration>();
            options.Context.Services.AddDbContext<UserAccountsDbContext>(o => o
                .UseSqlServer(connectionString.EmptyToNull() ?? entityFrameworkConfiguration.ConnectionString.EmptyToNull() ?? $"Server=(localdb)\\mssqllocaldb;Database={nameof(UserAccountsDbContext)};Trusted_Connection=True;MultipleActiveResultSets=True;", o => o
                    .EnableRetryOnFailure())
                .UseLoggerFactory(options.Context.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

            options.Context.Services.AddStartupTask<ApplyPendingMigrationsTask<UserAccountsDbContext>>();
            options.Context.Services.AddStartupTask<EchoStartupTask>(new TimeSpan(0, 0, 3));

            options.Context.Services.AddHealthChecks()
                .AddSqlServer(entityFrameworkConfiguration.ConnectionString, name: "UserAccounts-sqlserver");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: useraccounts service added");

            return options;
        }
    }
}

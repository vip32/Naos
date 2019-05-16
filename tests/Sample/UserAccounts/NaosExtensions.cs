namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.Infrastructure.EntityFramework;

    /// <summary>
    /// Naos service extensions.
    /// </summary>
    public static partial class NaosExtensions
    {
        public static ServiceOptions AddSampleUserAccounts(
            this ServiceOptions options,
            string connectionString = null,
            string section = "naos:sample:userAccounts:entityFramework",
            UserAccountsContext dbContext = null)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("UserAccounts");

            if(dbContext != null)
            {
                options.Context.Services.AddSingleton(dbContext); // cross wiring, warning this will be a singleton (not scoped)
            }

            options.Context.Services.AddScoped<IGenericRepository<UserAccount>>(sp =>
            {
                return new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        sp.GetRequiredService<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test", // TODO: resolve from runtime context
                            new RepositorySoftDeleteDecorator<UserAccount>(
                                new EntityFrameworkRepository<UserAccount>(o => o
                                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                    .Mediator(sp.GetRequiredService<IMediator>())
                                    .DbContext(sp.GetRequiredService<UserAccountsContext>()))))));
            });

            var entityFrameworkConfiguration = options.Context.Configuration?.GetSection(section).Get<EntityFrameworkConfiguration>();
            options.Context.Services
                .AddDbContext<UserAccountsContext>(o =>
            {
                o.UseLoggerFactory(options.Context.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>());
                //o.UseNaosSqlServer(entityFrameworkConfiguration.ConnectionString); // TODO: do we need this abstraction? as everything can be setup here (o.???)
                o.UseSqlServer(connectionString ?? entityFrameworkConfiguration.ConnectionString ?? $"Server=(localdb)\\mssqllocaldb;Database={nameof(UserAccountsContext)};Trusted_Connection=True;MultipleActiveResultSets=True;");
                //o.UseQueryTrackingBehavior(EntityFrameworkCore.QueryTrackingBehavior.NoTracking);
                o.ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning));
                o.EnableSensitiveDataLogging();
                o.EnableDetailedErrors();
            });

            options.Context.Services.AddStartupTask<ApplyPendingMigrationsTask<UserAccountsContext>>();
            options.Context.Services.AddStartupTask<Naos.Core.Common.Web.EchoStartupTask>();

            options.Context.Services.AddHealthChecks()
                .AddSqlServer(entityFrameworkConfiguration.ConnectionString, name: "UserAccounts-sqlserver");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: useraccounts service added");

            return options;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.EntityFramework;

    public static partial class NaosExtensions
    {
        public static ServiceOptions AddSampleUserAccounts(
            this ServiceOptions options,
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

            options.Context.Services.AddScoped<IUserAccountRepository>(sp =>
            {
                return new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        sp.GetRequiredService<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test", // TODO: resolve from runtime context
                            new EntityFrameworkRepository<UserAccount>(o => o
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Mediator(sp.GetRequiredService<IMediator>())
                                .DbContext(sp.GetRequiredService<UserAccountsContext>())))));
            });

            var entityFrameworkConfiguration = options.Context.Configuration?.GetSection(section).Get<EntityFrameworkConfiguration>();
            options.Context.Services.AddDbContext<UserAccountsContext>(o =>
            {
                o.UseLoggerFactory(options.Context.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>());
                o.UseNaosSqlServer(entityFrameworkConfiguration.ConnectionString);
                o.UseQueryTrackingBehavior(EntityFrameworkCore.QueryTrackingBehavior.NoTracking);
            });

            options.Context.Services.AddHealthChecks()
                .AddSqlServer(entityFrameworkConfiguration.ConnectionString, name: "UserAccounts-sqlserver");

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: useraccounts service added");

            return options;
        }
    }
}

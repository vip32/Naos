namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.EntityFramework;

    public static partial class ServiceExtensions
    {
        public static IServiceCollection AddSampleUserAccounts(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:sample:userAccounts:entityFramework",
            UserAccountsContext dbContext = null)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            if (dbContext != null)
            {
                services.AddSingleton(dbContext); // cross wiring, warning this will be a singleton (not scoped)
            }

            services.AddScoped<IUserAccountRepository>(sp =>
            {
                return new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        sp.GetRequiredService<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test", // TODO: resolve from runtime context
                            new EntityFrameworkRepository<UserAccount>(
                                sp.GetRequiredService<ILogger<UserAccountRepository>>(), // TODO: obsolete
                                sp.GetRequiredService<IMediator>(),
                                sp.GetRequiredService<UserAccountsContext>()))));
            });

            var entityFrameworkConfiguration = configuration?.GetSection(section).Get<EntityFrameworkConfiguration>();
            services.AddDbContext<UserAccountsContext>(options => options.UseNaosSqlServer(entityFrameworkConfiguration.ConnectionString)); // needed for migrations:add/update
            services.AddHealthChecks()
                .AddSqlServer(entityFrameworkConfiguration.ConnectionString, name: $"UserAccounts-sqlserver");

            return services;
        }
    }
}

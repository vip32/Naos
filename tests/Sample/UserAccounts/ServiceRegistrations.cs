namespace Naos.Sample.UserAccounts
{
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.EntityFramework;
    using SimpleInjector;

    public static class ServiceRegistrations
    {
        public static Container AddSampleUserAccounts(
            this Container container,
            IConfiguration configuration,
            IServiceCollection services,
            string section = "naos:sample:userAccounts:entityFramework",
            UserAccountsContext dbContext = null)
        {
            if (dbContext != null)
            {
                container.RegisterInstance(dbContext); // cross wiring, warning this will be a singleton (not scoped)
            }

            container.Register<IUserAccountRepository>(() =>
            {
                return new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        container.GetInstance<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test", // TODO: resolve from runtime context
                            new EntityFrameworkRepository<UserAccount>(
                                container.GetInstance<ILogger<UserAccountRepository>>(), // TODO: obsolete
                                container.GetInstance<IMediator>(),
                                container.GetInstance<UserAccountsContext>()))));
            });

            var entityFrameworkConfiguration = configuration.GetSection(section).Get<EntityFrameworkConfiguration>();
            services?.AddDbContext<UserAccountsContext>(options => options.UseNaosSqlServer(entityFrameworkConfiguration.ConnectionString)); // needed for migrations:add/update
            services?.AddHealthChecks()
                .AddSqlServer(entityFrameworkConfiguration.ConnectionString);

            return container;
        }
    }
}

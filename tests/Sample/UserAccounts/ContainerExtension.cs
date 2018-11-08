namespace Naos.Sample.UserAccounts
{
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.Infrastructure;
    using SimpleInjector;

    public static class ContainerExtension
    {
        public static Container AddSampleUserAccounts(
            this Container container,
            IConfiguration configuration,
            string section = "naos:sample:userAccounts:entityFramework")
        {
            var entityFrameworkConfiguration = configuration.GetSection(section).Get<EntityFrameworkConfiguration>();
            Ensure.That(entityFrameworkConfiguration).IsNotNull();

            container.RegisterSingleton<IUserAccountRepository>(() =>
            {
                return new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        container.GetInstance<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test",
                            new EntityFrameworkRepository<UserAccount>(
                                container.GetInstance<ILogger<UserAccountRepository>>(), // TODO: obsolete
                                container.GetInstance<IMediator>(),
                                new UserAccountContext(
                                    new DbContextOptionsBuilder<UserAccountContext>()
                                        .UseSqlServer(entityFrameworkConfiguration.ConnectionString)
                                        .Options)))));
            });

            var inMemoryRepository = new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        container.GetInstance<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test",
                            new InMemoryRepository<UserAccount>(
                                container.GetInstance<IMediator>()))));

            var inMemoryWithMappedRepository = new UserAccountRepository(
                    new RepositoryLoggingDecorator<UserAccount>(
                        container.GetInstance<ILogger<UserAccountRepository>>(),
                        new RepositoryTenantDecorator<UserAccount>(
                            "naos_sample_test",
                            new InMemoryRepository<UserAccount>(
                                container.GetInstance<IMediator>()))));

            return container;
        }
    }
}

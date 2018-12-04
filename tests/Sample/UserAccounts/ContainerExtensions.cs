namespace Naos.Sample.UserAccounts
{
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Infrastructure.EntityFramework;
    using Naos.Sample.UserAccounts.Domain;
    using Naos.Sample.UserAccounts.EntityFramework;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container AddSampleUserAccounts(
            this Container container,
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

            return container;
        }
    }
}

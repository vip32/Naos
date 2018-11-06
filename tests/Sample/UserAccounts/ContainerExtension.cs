namespace Naos.Sample.UserAccounts
{
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
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
                new UserAccountRepository(
                    container.GetInstance<ILogger<UserAccountRepository>>(),
                    container.GetInstance<IMediator>(),
                    new UserAccountContext(
                        new DbContextOptionsBuilder<UserAccountContext>()
                            .UseSqlServer(entityFrameworkConfiguration.ConnectionString)
                            .Options))); // "Server=(localdb)\\mssqllocaldb;Database=naos;Trusted_Connection=true;"

            return container;
        }
    }
}

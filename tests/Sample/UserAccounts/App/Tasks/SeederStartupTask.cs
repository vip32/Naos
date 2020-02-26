namespace Naos.Sample.UserAccounts.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Sample.UserAccounts.Domain;

    public class SeederStartupTask : IStartupTask
    {
        private readonly ILogger<SeederStartupTask> logger;
        private readonly IGenericRepository<UserAccount> repository;

        public SeederStartupTask(ILoggerFactory loggerFactory, IGenericRepository<UserAccount> repository)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(repository, nameof(repository));

            this.logger = loggerFactory.CreateLogger<SeederStartupTask>();
            this.repository = repository;
        }

        public TimeSpan? Delay { get; set; } = new TimeSpan(0, 0, 5);

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            //this.logger.LogInformation("{LogKey:l} task started: useraccounts seeder", LogKeys.StartupTask);

            new List<UserAccount>
            {
                //new UserAccount() { Id = Guid.Parse("100fb10f-2ad4-4bd1-9b33-6410a5ce7b25"), Email = "admin@naos.com", TenantId = "naos_sample_test", Status = UserAccountStatus.Active },
                //new UserAccount() { Id = Guid.Parse("100fb10f-2ad4-4bd1-9b33-6410a5ce7b26"), Email = "test26@unknown.com", TenantId = "naos_sample_test", Status = UserAccountStatus.Active },
                //new UserAccount() { Id = Guid.Parse("100fb10f-2ad4-4bd1-9b33-6410a5ce7b27"), Email = "test27@unknown.com", TenantId = "naos_sample_test", Status = UserAccountStatus.Active },
            }.ForEach(async e =>
            {
                try
                {
                    if (!await this.repository.ExistsAsync(e.Id).AnyContext())
                    {
                        await this.repository.InsertAsync(e).AnyContext();
                    }
                }
                catch// (Exception ex)
                {
                    //this.logger.LogWarning(ex, $"{{LogKey:l}} cannot seed entity (type=ProductInventory) {ex.Message}", LogKeys.Startup);
                }
            });

            return Task.CompletedTask;
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

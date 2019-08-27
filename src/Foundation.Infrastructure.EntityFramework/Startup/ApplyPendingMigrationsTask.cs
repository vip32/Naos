namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Application;

    public class ApplyPendingMigrationsTask<TDbContext> : IStartupTask
    where TDbContext : DbContext
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ApplyPendingMigrationsTask<TDbContext>> logger;

        public ApplyPendingMigrationsTask(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
            this.logger = loggerFactory.CreateLogger<ApplyPendingMigrationsTask<TDbContext>>();
        }

        //public TimeSpan? Delay => TimeSpan.Zero;
        public TimeSpan? Delay { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            // create a new scope to retrieve scoped services
            using (var scope = this.serviceProvider.CreateScope())
            {
                try
                {
                    await scope.ServiceProvider
                        .GetRequiredService<TDbContext>()
                        .Database.MigrateAsync().AnyContext();
                }
                catch (SqlException ex)
                {
                    this.logger.LogError(ex, $"{{LogKey:l}} database migration failed: {ex.Message}", LogKeys.StartupTask);
                }
            }
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

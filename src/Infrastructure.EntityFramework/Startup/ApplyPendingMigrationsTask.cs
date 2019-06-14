namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation.Application;

    public class ApplyPendingMigrationsTask<TDbContext> : IStartupTask
    where TDbContext : DbContext
    {
        private readonly IServiceProvider serviceProvider;

        public ApplyPendingMigrationsTask(IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            // create a new scope to retrieve scoped services
            using(var scope = this.serviceProvider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<TDbContext>()
                    .Database.MigrateAsync().AnyContext();
            }
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

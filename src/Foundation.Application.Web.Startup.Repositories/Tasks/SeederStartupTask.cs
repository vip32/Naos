namespace Naos.Foundation.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class SeederStartupTask<TRepository, TEntity> : IStartupTask
        where TRepository : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<SeederStartupTask<TRepository, TEntity>> logger;

        public SeederStartupTask(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
            this.logger = loggerFactory.CreateLogger<SeederStartupTask<TRepository, TEntity>>();
        }

        public TimeSpan? Delay { get; set; }

        public IEnumerable<TEntity> Entities { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            // create a new scope to retrieve scoped services
            using (var scope = this.serviceProvider.CreateScope())
            {
                try
                {
                    var entityTypeName = typeof(TEntity).PrettyName();
                    this.logger.LogInformation($"{{LogKey:l}} repository seeder start entities: {entityTypeName}", LogKeys.StartupTask);
                    var repository = scope.ServiceProvider.GetRequiredService<TRepository>();
                    foreach (var entity in this.Entities.Safe().Where(e => e.Id != null))
                    {
                        // insert only
                        if (!await repository.ExistsAsync(entity.Id).AnyContext())
                        {
                            if (entity is IStateEntity stateEntity)
                            {
                                stateEntity.State.SetCreated();
                            }

                            this.logger.LogInformation($"{{LogKey:l}} repository seeder insert entity: {entityTypeName} (id={entity.Id})", LogKeys.StartupTask);
                            await repository.InsertAsync(entity).AnyContext();
                        }
                    }
                }
                catch (Exception ex) // was SqlException
                {
                    this.logger.LogError(ex, $"{{LogKey:l}} database seeder failed: {ex.GetFullMessage()}", LogKeys.StartupTask);
                }
            }
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

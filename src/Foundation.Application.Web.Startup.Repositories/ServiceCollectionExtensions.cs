namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.Foundation.Domain;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add an <see cref="IStartupTask"/> registration for the given type.
        /// </summary>
        ///
        /// .-----------.    .-----------.       .----------------.             .-----------.   .-----------.
        /// | Program   |    | WebHost   |       | StartupTask    |             | MyTask    |   | Kester    |
        /// `-----------`    `-----------`       | ServerDecorator|             `-----------`   | Server    |
        ///       |               |              `----------------`                 |           `-----------`
        ///       |  RunAsync()   |                     |  [decorator]              |                |  [decoratee]
        ///       |-------------->|=--.                 |                           |                |
        ///       |               |=  | Build           |                           |                |
        ///       |               |=  | App             |                           |                |
        ///       |               |=<-"                 |                           |                |
        ///       |               |   StartAsync()      |                           |                |
        ///       |               |-------------------->|   StartAsync()            |                |
        ///       |               |                     |-------------------------->|  StartAsync()  |
        ///       |               |                     |                           |--------------->|
        ///       |               |                     |                           |                |
        ///
        /// <typeparam name="TStartupTask">An <see cref="IStartupTask"/> to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSeederStartupTask<TEntity, TRepository>(this IServiceCollection services, IEnumerable<TEntity> entities, TimeSpan? delay = null)
            where TRepository : IGenericRepository<TEntity>
            where TEntity : class, IEntity, IAggregateRoot
                => services
                    .AddStartupTaskServerDecorator()
                    .AddTransient<IStartupTask>(sp =>
                    {
                        var task = ActivatorUtilities.GetServiceOrCreateInstance<SeederStartupTask<TEntity, TRepository>>(sp);
                        if (task != null && delay.HasValue)
                        {
                            task.Delay = delay;
                        }

                        if (task != null)
                        {
                            task.Entities = entities;
                        }

                        return task;
                    });
    }
}

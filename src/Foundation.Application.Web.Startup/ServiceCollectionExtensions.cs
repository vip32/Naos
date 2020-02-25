namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Foundation;

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
        public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services)
            where TStartupTask : class, IStartupTask
                => services
                    .AddStartupTaskServerDecorator()
                    .AddTransient<IStartupTask, TStartupTask>();

        public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services, Func<IServiceProvider, TStartupTask> implementationFactory)
            where TStartupTask : class, IStartupTask
                => services
                    .AddStartupTaskServerDecorator()
                    .AddTransient<IStartupTask>(sp => implementationFactory(sp));

        public static IServiceCollection AddStartupTask<TStartupTask>(this IServiceCollection services, TimeSpan delay)
            where TStartupTask : class, IStartupTask
                => services
                    .AddStartupTaskServerDecorator()
                    .AddTransient<IStartupTask>(sp =>
                    {
                        var task = ActivatorUtilities.GetServiceOrCreateInstance<TStartupTask>(sp);
                        if (task != null)
                        {
                            task.Delay = delay;
                        }

                        return task;
                    });
    }
}

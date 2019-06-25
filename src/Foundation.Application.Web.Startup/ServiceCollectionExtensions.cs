namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation.Application;

    public static class ServiceCollectionExtensions
    {
        private static bool isDecorated;

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

        private static IServiceCollection AddStartupTaskServerDecorator(this IServiceCollection services)
        {
            if(isDecorated)
            {
                return services; // already decorated the IServer (idempotent)
            }

            var serviceDescriptor = GetIServerDescriptor(services);
            if(serviceDescriptor == null)
            {
                return services; // just return as there is no iservice (integration tests)
                //throw new Exception("Could not find any registered services for type IServer. IStartupTask requires using an IServer");
            }

            var decoratedServiceDescriptor = CreateDecoratedServiceDescriptor(serviceDescriptor, typeof(StartupTaskServerDecorator));
            var index = services.IndexOf(serviceDescriptor);
            services.Insert(index, decoratedServiceDescriptor); // avoid reordering descriptors
            services.Remove(serviceDescriptor);

            isDecorated = true;
            return services;
        }

        // from https://github.com/khellang/Scrutor/blob/5516fe092594c5063f6ab885890b79b2bf91cc24/src/Scrutor/ServiceCollectionExtensions.Decoration.cs
        private static ServiceDescriptor GetIServerDescriptor(IServiceCollection services)
        {
            return services.FirstOrDefault(service => service.ServiceType == typeof(IServer));
        }

        private static ServiceDescriptor CreateDecoratedServiceDescriptor(this ServiceDescriptor descriptor, Type decorateeType)
        {
            return ServiceDescriptor.Describe(
                descriptor.ServiceType,
                provider => provider.CreateInstance(decorateeType, provider.GetInstance(descriptor)),
                descriptor.Lifetime);
        }

        private static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor)
        {
            if(descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if(descriptor.ImplementationType != null)
            {
                return provider.GetServiceOrCreateInstance(descriptor.ImplementationType);
            }

            return descriptor.ImplementationFactory(provider);
        }

        private static object GetServiceOrCreateInstance(this IServiceProvider provider, Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
        }

        private static object CreateInstance(this IServiceProvider provider, Type type, params object[] arguments)
        {
            return ActivatorUtilities.CreateInstance(provider, type, arguments);
        }
    }
}

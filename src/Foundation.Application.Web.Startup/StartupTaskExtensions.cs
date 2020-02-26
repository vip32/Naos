namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting.Server;
    using Naos.Foundation;
    using Naos.Foundation.Application;

    public static class StartupTaskExtensions
    {
        public static bool IsDecorated { get; set; } // ND1901:AvoidNonReadOnlyStaticFields, however is assigned only once to keep track of serviceprovider (decorator) state

        public static IServiceCollection AddStartupTask(this IServiceCollection services, Type type, TimeSpan? delay = null)
                => services
                    .AddStartupTaskServerDecorator()
                    .AddTransient<IStartupTask>(sp =>
                    {
                        var task = ActivatorUtilities.GetServiceOrCreateInstance(sp, type) as IStartupTask;
                        if (task != null && delay.HasValue)
                        {
                            task.Delay = delay.Value;
                        }

                        return task;
                    });

        public static IServiceCollection AddStartupTaskServerDecorator(this IServiceCollection services)
        {
            if (IsDecorated)
            {
                return services; // already decorated the IServer (idempotent)
            }

            var serviceDescriptor = GetDefaultServerDescriptor(services);
            if (serviceDescriptor == null)
            {
                return services; // just return as there is no iservice (integration tests)
                //throw new Exception("Could not find any registered services for type IServer. IStartupTask requires using an IServer");
            }

            // replace the default ServiceDescriptor with a decorated one (for task execution)
            var decoratedServiceDescriptor = CreateDecoratedServiceDescriptor(serviceDescriptor, typeof(StartupTaskServerDecorator));
            var index = services.IndexOf(serviceDescriptor);
            services.Insert(index, decoratedServiceDescriptor); // avoid reordering descriptors
            services.Remove(serviceDescriptor);

            IsDecorated = true;
            return services;
        }

        // from https://github.com/khellang/Scrutor/blob/5516fe092594c5063f6ab885890b79b2bf91cc24/src/Scrutor/ServiceCollectionExtensions.Decoration.cs
        private static ServiceDescriptor GetDefaultServerDescriptor(IServiceCollection services)
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
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationType != null)
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

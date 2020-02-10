namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Messaging.Application.Web;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueueProcessingStartupTask<T>(this IServiceCollection services)
            where T : class
                => services.AddStartupTask<QueueProcessingStartupTask<T>>();

        public static IServiceCollection AddQueueProcessingStartupTask<T>(this IServiceCollection services, TimeSpan delay)
            where T : class
                => services.AddStartupTask<QueueProcessingStartupTask<T>>(delay);

        public static IServiceCollection AddQueueProcessingStartupTask(this IServiceCollection services, Type type, TimeSpan? delay)
                => services.AddStartupTask(type, delay);
    }
}

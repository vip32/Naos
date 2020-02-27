namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Naos.Messaging.Application.Web;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueueProcessItemsStartupTask<TData>(this IServiceCollection services)
            where TData : class
                => services.AddStartupTask<QueueProcessItemsStartupTask<TData>>();

        public static IServiceCollection AddQueueProcessItemsStartupTask<TData>(this IServiceCollection services, TimeSpan delay)
            where TData : class
                => services.AddStartupTask<QueueProcessItemsStartupTask<TData>>(delay);

        public static IServiceCollection AddQueueProcessItemsStartupTask(this IServiceCollection services, Type type, TimeSpan? delay)
                => services.AddStartupTask(type, delay);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Naos.Queueing.Application;
    using Naos.Queueing.Application.Web;
    using Naos.Queueing.Domain;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        /// <summary>
        /// Register startuptask which processes TData items
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="options"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static QueueingProviderOptions<TData> ProcessItems<TData>(
            this QueueingProviderOptions<TData> options,
            TimeSpan? delay = null)
            where TData : class
        {
            options.Context.Services.AddQueueProcessItemsStartupTask<TData>(delay ?? new TimeSpan(0, 0, 15));

            return options;
        }

        /// <summary>
        /// Register hosted service which processes TData items
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="options"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static QueueingProviderOptions<TData> ProcessItemsHostedService<TData>(
            this QueueingProviderOptions<TData> options,
            Func<IQueueItem<TData>, Task> handler = null)
            where TData : class
        {
            if (handler != null)
            {
                options.Context.Services.AddSingleton(handler);
            }

            options.Context.Services.AddHostedService<QueueProcessItemsHostedService<TData>>();

            return options;
        }
    }
}

namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class QueueExtensions
    {
        public static Task ProcessItemsAsync<TData>(
            this IQueue<TData> source,
            Func<IQueueItem<TData>, Task> handler,
            bool autoComplete = false,
            CancellationToken cancellationToken = default)
            where TData : class
            => source.ProcessItemsAsync((entry, token) => handler(entry), autoComplete, cancellationToken);

        //public static Task ProcessItemsAsync<TData>(
        //    this IQueue<TData> source,
        //    bool autoComplete = false,
        //    CancellationToken cancellationToken = default)
        //    where TData : class
        //    => source.ProcessItemsAsync(autoComplete, cancellationToken);
    }
}

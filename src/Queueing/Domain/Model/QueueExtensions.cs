namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class QueueExtensions
    {
        public static Task ProcessItemsAsync<T>(
            this IQueue<T> source,
            Func<IQueueItem<T>, Task> handler,
            bool autoComplete = false,
            CancellationToken cancellationToken = default)
            where T : class
            => source.ProcessItemsAsync((entry, token) => handler(entry), autoComplete, cancellationToken);
    }
}

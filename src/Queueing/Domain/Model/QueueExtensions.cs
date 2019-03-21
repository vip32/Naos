namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class QueueExtensions
    {
        /// <summary>
        /// Asynchronously dequeues items in the background. Dequeued items are handled by the specified handler
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="source">The <see cref="IQueue"/> instance</param>
        /// <param name="handler">Function called on the dequeued item</param>
        /// <param name="autoComplete">True to call <see cref="CompleteAsync"/> after the <paramref name="handler"/> is run, defaults to false</param>
        /// <param name="cancellationToken">The token used to cancel the background worker</param>
        /// <returns></returns>
        public static Task ProcessItemsAsync<TData>(
            this IQueue<TData> source,
            Func<IQueueItem<TData>, Task> handler,
            bool autoComplete = false,
            CancellationToken cancellationToken = default)
            where TData : class
            => source.ProcessItemsAsync((entry, token) => handler(entry), autoComplete, cancellationToken);
    }
}

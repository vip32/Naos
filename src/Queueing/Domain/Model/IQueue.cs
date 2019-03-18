namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueue
    {
        string Name { get; }

        DateTime? LastEnqueuedDate { get; }

        DateTime? LastDequeuedDate { get; }

        /// <summary>
        /// Gets the queue metrics
        /// </summary>
        /// <returns></returns>
        Task<QueueMetrics> GetMetricsAsync();

        /// <summary>
        /// Deletes the physical queue
        /// </summary>
        /// <returns></returns>
        Task DeleteQueueAsync();
    }

    public interface IQueue<T> : IQueue, IDisposable
        where T : class
    {
        /// <summary>
        /// Enqueues an item
        /// </summary>
        /// <param name="data">the data to queue</param>
        /// <returns></returns>
        Task<string> EnqueueAsync(T data);

        /// <summary>
        /// Dequeues an item, if no item queued it will wait for the speficied amout (timeout)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IQueueItem<T>> DequeueAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Dequeues an item, if no item queued it will wait for the speficied amout (timeout)
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null);

        /// <summary>
        /// Renews the lock on a dequeued item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task RenewLockAsync(IQueueItem<T> item);

        /// <summary>
        /// Completes a dequeued item, take it out of the queue permanently
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task CompleteAsync(IQueueItem<T> item);

        /// <summary>
        /// Cancel a dequeued item, puts it back in the queue again
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task AbandonAsync(IQueueItem<T> item);

        /// <summary>Asynchronously dequeues entries in the background.</summary>
        /// <param name="handler">Function called on entry dequeued</param>
        /// <param name="autoComplete">True to call <see cref="CompleteAsync"/> after the <paramref name="handler"/> is run, defaults to false</param>
        /// <param name="cancellationToken">The token used to cancel the background worker</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ProcessItemsAsync(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete = false, CancellationToken cancellationToken = default);
    }
}

namespace Naos.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileStorageQueueDecorator<T> : IQueue<T>
        where T : class
    {
        private readonly IQueue<T> inner;
        //private readonly IFileStorage fileStorage;

        public FileStorageQueueDecorator(IQueue<T> decoratee) /*, IFileStorage fileStorage)*/ // TODO: each enqueued item will also be stored by using the FileStorage instance
        {
            EnsureThat.EnsureArg.IsNotNull(decoratee, nameof(decoratee));
            //EnsureThat.EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.inner = decoratee;
            //this.fileStorage = fileStorage;
        }

        public string Name => this.inner.Name;

        public DateTime? LastEnqueuedDate => this.inner.LastEnqueuedDate;

        public DateTime? LastDequeuedDate => this.inner.LastDequeuedDate;

        public Task AbandonAsync(IQueueItem<T> item)
        {
            return this.inner.AbandonAsync(item);
        }

        public Task CompleteAsync(IQueueItem<T> item)
        {
            return this.inner.CompleteAsync(item);
        }

        public Task DeleteQueueAsync()
        {
            return this.inner.DeleteQueueAsync();
        }

        public Task<IQueueItem<T>> DequeueAsync(CancellationToken cancellationToken)
        {
            return this.inner.DequeueAsync(cancellationToken);
        }

        public Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null)
        {
            return this.inner.DequeueAsync(timeout);
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public Task<string> EnqueueAsync(T data)
        {
            return this.inner.EnqueueAsync(data);
        }

        public Task<QueueMetrics> GetMetricsAsync()
        {
            return this.inner.GetMetricsAsync();
        }

        public Task ProcessItemsAsync(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return this.inner.ProcessItemsAsync(handler, autoComplete, cancellationToken);
        }

        public Task ProcessItemsAsync(bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return this.inner.ProcessItemsAsync(autoComplete, cancellationToken);
        }

        public Task RenewLockAsync(IQueueItem<T> item)
        {
            return this.inner.RenewLockAsync(item);
        }
    }
}

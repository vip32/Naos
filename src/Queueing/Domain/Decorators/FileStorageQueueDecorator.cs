namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileStorageQueueDecorator<T> : IQueue<T>
        where T : class
    {
        private readonly IQueue<T> decoratee;
        //private readonly IFileStorage fileStorage;

        public FileStorageQueueDecorator(IQueue<T> decoratee) /*, IFileStorage fileStorage)*/ // TODO: each enqueued item will also be stored by using the FileStorage instance
        {
            EnsureThat.EnsureArg.IsNotNull(decoratee, nameof(decoratee));
            //EnsureThat.EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.decoratee = decoratee;
            //this.fileStorage = fileStorage;
        }

        public string Name => this.decoratee.Name;

        public DateTime? LastEnqueuedDate => this.decoratee.LastEnqueuedDate;

        public DateTime? LastDequeuedDate => this.decoratee.LastDequeuedDate;

        public Task AbandonAsync(IQueueItem<T> item)
        {
            return this.decoratee.AbandonAsync(item);
        }

        public Task CompleteAsync(IQueueItem<T> item)
        {
            return this.decoratee.CompleteAsync(item);
        }

        public Task DeleteQueueAsync()
        {
            return this.decoratee.DeleteQueueAsync();
        }

        public Task<IQueueItem<T>> DequeueAsync(CancellationToken cancellationToken)
        {
            return this.decoratee.DequeueAsync(cancellationToken);
        }

        public Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null)
        {
            return this.decoratee.DequeueAsync(timeout);
        }

        public void Dispose()
        {
            this.decoratee.Dispose();
        }

        public Task<string> EnqueueAsync(T data)
        {
            return this.decoratee.EnqueueAsync(data);
        }

        public Task<QueueMetrics> GetMetricsAsync()
        {
            return this.decoratee.GetMetricsAsync();
        }

        public Task ProcessItemsAsync(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return this.decoratee.ProcessItemsAsync(handler, true, cancellationToken);
        }

        public Task RenewLockAsync(IQueueItem<T> item)
        {
            return this.decoratee.RenewLockAsync(item);
        }
    }
}

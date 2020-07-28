namespace Naos.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Foundation;

    public class AutoCompleteQueueDecorator<T> : IQueue<T>
        where T : class
    {
        private readonly IQueue<T> inner;

        public AutoCompleteQueueDecorator(IQueue<T> decoratee)
        {
            EnsureThat.EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.inner = decoratee;
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

        public async Task<IQueueItem<T>> DequeueAsync(CancellationToken cancellationToken)
        {
            var item = await this.inner.DequeueAsync(cancellationToken).AnyContext();
            await this.inner.CompleteAsync(item).AnyContext();

            return item;
        }

        public Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null)
        {
            return this.inner.DequeueAsync(timeout);
            // no need to complete as this is done by DequeueAsync(CancellationToken cancellationToken)
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
            return this.inner.ProcessItemsAsync(handler, true, cancellationToken);
        }

        public Task ProcessItemsAsync(bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return this.inner.ProcessItemsAsync(true, cancellationToken);
        }

        public Task RenewLockAsync(IQueueItem<T> item)
        {
            return this.inner.RenewLockAsync(item);
        }
    }
}

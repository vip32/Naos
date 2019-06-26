namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Foundation;

    public class AutoCompleteQueueDecorator<T> : IQueue<T>
        where T : class
    {
        private readonly IQueue<T> decoratee;

        public AutoCompleteQueueDecorator(IQueue<T> decoratee)
        {
            EnsureThat.EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.decoratee = decoratee;
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

        public async Task<IQueueItem<T>> DequeueAsync(CancellationToken cancellationToken)
        {
            var item = await this.decoratee.DequeueAsync(cancellationToken).AnyContext();
            await this.decoratee.CompleteAsync(item).AnyContext();

            return item;
        }

        public Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null)
        {
            return this.decoratee.DequeueAsync(timeout);
            // no need to complete as this is done by DequeueAsync(CancellationToken cancellationToken)
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

        public Task ProcessItemsAsync(bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return this.decoratee.ProcessItemsAsync(true, cancellationToken);
        }

        public Task RenewLockAsync(IQueueItem<T> item)
        {
            return this.decoratee.RenewLockAsync(item);
        }
    }
}

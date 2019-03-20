namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public abstract class BaseQueue<T, TOptions> : IQueue<T>
        where T : class
        where TOptions : BaseQueueOptions, new()
    {
        protected readonly TOptions options;
        protected readonly ILogger<T> logger;
        protected readonly ISerializer serializer;
        protected readonly CancellationTokenSource disposedCancellationTokenSource;
        private bool isDisposed;

        protected BaseQueue(TOptions options)
        {
            this.options = options ?? Factory<TOptions>.Create();
            this.logger = options.LoggerFactory.CreateLogger<T>();
            this.serializer = options.Serializer ?? DefaultSerializer.Instance;
            options.Name = options.Name ?? typeof(T).PrettyName().Replace("<", "_").Replace(">", "_").ToLower().Pluralize();
            this.Name = options.Name;
            this.disposedCancellationTokenSource = new CancellationTokenSource();
        }

        public string Name { get; protected set; }

        public DateTime? LastEnqueuedDate { get; protected set; }

        public DateTime? LastDequeuedDate { get; protected set; }

        public abstract Task<string> EnqueueAsync(T data);

        public virtual async Task<IQueueItem<T>> DequeueAsync(CancellationToken cancellationToken)
        {
            using (var linkedCancellationToken = this.CreateLinkedTokenSource(cancellationToken))
            {
                await this.EnsureQueueAsync(linkedCancellationToken.Token).AnyContext();

                this.LastDequeuedDate = DateTime.UtcNow;
                return await this.DequeueWithIntervalAsync(linkedCancellationToken.Token).AnyContext();
            }
        }

        public virtual async Task<IQueueItem<T>> DequeueAsync(TimeSpan? timeout = null)
        {
            using (var cancellationTokenSource = timeout.ToCancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                return await this.DequeueAsync(cancellationTokenSource.Token).AnyContext();
            }
        }

        public abstract Task RenewLockAsync(IQueueItem<T> item);

        public abstract Task CompleteAsync(IQueueItem<T> item);

        public abstract Task AbandonAsync(IQueueItem<T> item);

        public abstract Task<QueueMetrics> GetMetricsAsync();

        public abstract Task ProcessItemsAsync(Func<IQueueItem<T>, CancellationToken, Task> handler, bool autoComplete = false, CancellationToken cancellationToken = default);

        public abstract Task DeleteQueueAsync();

        public virtual void Dispose()
        {
            if (!this.isDisposed)
            {
                this.logger.LogDebug($"dispose queue {this.Name}");
                this.isDisposed = true;
                this.disposedCancellationTokenSource?.Cancel();
                this.disposedCancellationTokenSource?.Dispose();
            }
        }

        protected abstract Task<IQueueItem<T>> DequeueWithIntervalAsync(CancellationToken cancellationToken);

        protected CancellationTokenSource CreateLinkedTokenSource(CancellationToken cancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(this.disposedCancellationTokenSource.Token, cancellationToken);
        }

        protected abstract Task EnsureQueueAsync(CancellationToken cancellationToken = default);
    }
}

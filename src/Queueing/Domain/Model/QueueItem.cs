namespace Naos.Queueing.Domain
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    [DebuggerDisplay("Id={Id}, IsCompleted={IsCompleted}")]
    public class QueueItem<TData> : IQueueItem<TData>
        where TData : class
    {
        private readonly IQueue<TData> queue;

        public QueueItem(string id, TData value, IQueue<TData> queue, DateTime enqueued, int attempts)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(id, nameof(id));
            EnsureThat.EnsureArg.IsNotNull(queue, nameof(queue));

            this.Id = id;
            this.Data = value;
            this.queue = queue;
            this.EnqueuedDate = enqueued;
            this.Attempts = attempts;
            this.DequeuedDate = this.RenewedDate = DateTime.UtcNow;
        }

        public string Id { get; }

        public bool IsCompleted { get; private set; }

        public bool IsAbandoned { get; private set; }

        public TData Data { get; set; }

        public DateTime EnqueuedDate { get; set; }

        public DateTime RenewedDate { get; set; }

        public DateTime DequeuedDate { get; set; }

        public int Attempts { get; set; }

        public TimeSpan ProcessingTime { get; set; }

        public TimeSpan TotalTime { get; set; }

        public DataDictionary Properties { get; } = new DataDictionary();

        public void MarkCompleted()
        {
            this.IsCompleted = true;
        }

        public void MarkAbandoned()
        {
            this.IsAbandoned = true;
        }

        public Task RenewLockAsync()
        {
            this.RenewedDate = DateTime.UtcNow;
            return this.queue.RenewLockAsync(this);
        }

        public Task CompleteAsync()
        {
            return this.queue.CompleteAsync(this);
        }

        public Task AbandonAsync()
        {
            return this.queue.AbandonAsync(this);
        }

        public void Dispose()
        {
            if (!this.IsAbandoned && !this.IsCompleted)
            {
                this.AbandonAsync();
            }
        }
    }
}

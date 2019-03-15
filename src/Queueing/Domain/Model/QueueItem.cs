namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Model;

    [DebuggerDisplay("Id={Id}, Queue={Name}")]
    public class QueueItem<T> : IQueueItem<T>
        where T : class
    {
        private readonly IQueue<T> queue;

        public QueueItem(string id, T value, IQueue<T> queue, DateTime enqueued, int attempts)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(id, nameof(id));
            EnsureThat.EnsureArg.IsNotNull(queue, nameof(queue));

            this.Id = id;
            this.Value = value;
            this.queue = queue;
            this.EnqueuedDate = enqueued;
            this.Attempts = attempts;
            this.DequeuedDate = this.RenewedDate = DateTime.UtcNow;
        }

        public string Id { get; }

        public bool IsCompleted { get; private set; }

        public bool IsAbandoned { get; private set; }

        public T Value { get; set; }

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

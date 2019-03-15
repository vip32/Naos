namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Model;

    public interface IQueueItem<T> : IDisposable
        where T : class
    {
        string Id { get; }

        T Value { get; }

        bool IsCompleted { get; }

        bool IsAbandoned { get; }

        DateTime EnqueuedDate { get; }

        DateTime RenewedDate { get; }

        DateTime DequeuedDate { get; }

        int Attempts { get; }

        TimeSpan ProcessingTime { get; }

        TimeSpan TotalTime { get; }

        DataDictionary Properties { get; }

        void MarkAbandoned();

        void MarkCompleted();

        Task RenewLockAsync();

        Task AbandonAsync();

        Task CompleteAsync();
    }
}

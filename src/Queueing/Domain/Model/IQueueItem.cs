namespace Naos.Queueing.Domain
{
    using System;
    using System.Threading.Tasks;
    using Naos.Foundation.Domain;

    public interface IQueueItem<TData> : IDisposable
        where TData : class
    {
        string Id { get; }

        TData Data { get; }

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

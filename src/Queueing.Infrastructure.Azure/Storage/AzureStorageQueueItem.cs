namespace Naos.Queueing.Infrastructure.Azure
{
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Naos.Queueing.Domain;

    public class AzureStorageQueueItem<TData> : QueueItem<TData>
        where TData : class
    {
        public AzureStorageQueueItem(CloudQueueMessage message, TData value, IQueue<TData> queue)
            : base(message.Id, value, queue, message.InsertionTime.GetValueOrDefault().UtcDateTime, message.DequeueCount)
        {
            EnsureArg.IsNotNull(message, nameof(message));

            this.Message = message;
        }

        public CloudQueueMessage Message { get; }
    }
}
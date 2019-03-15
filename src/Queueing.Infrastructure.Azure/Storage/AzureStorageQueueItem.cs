namespace Naos.Core.Queueing.Infrastructure.Azure
{
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Naos.Core.Queueing.Domain;

    public class AzureStorageQueueItem<T> : QueueItem<T>
        where T : class
    {
        public AzureStorageQueueItem(CloudQueueMessage message, T value, IQueue<T> queue)
            : base(message.Id, value, queue, message.InsertionTime.GetValueOrDefault().UtcDateTime, message.DequeueCount)
        {
            EnsureArg.IsNotNull(message, nameof(message));

            this.Message = message;
        }

        public CloudQueueMessage Message { get; }
    }
}
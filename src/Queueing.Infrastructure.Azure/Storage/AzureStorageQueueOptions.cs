namespace Naos.Core.Queueing.Infrastructure.Azure
{
    using System;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Naos.Core.Queueing.Domain;

    public class AzureStorageQueueOptions : QueueBaseOptions
    {
        public string ConnectionString { get; set; }

        public IRetryPolicy RetryPolicy { get; set; }

        public TimeSpan DequeueInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}

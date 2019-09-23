namespace Naos.Queueing.Infrastructure.Azure
{
    using System;
    using MediatR;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Naos.Queueing.Domain;

    public class AzureStorageQueueOptions : BaseQueueOptions
    {
        public IMediator Mediator { get; set; }

        public string ConnectionString { get; set; }

        public IRetryPolicy RetryPolicy { get; set; }

        public TimeSpan DequeueInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}

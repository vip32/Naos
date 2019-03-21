namespace Naos.Core.Queueing.Infrastructure.Azure
{
    using System;
    using MediatR;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class AzureStorageQueueOptionsBuilder :
       BaseOptionsBuilder<AzureStorageQueueOptions, AzureStorageQueueOptionsBuilder>
    {
        public AzureStorageQueueOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Name(string name)
        {
            this.Target.Name = name;
            return this;
        }

        public AzureStorageQueueOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            return this;
        }

        public AzureStorageQueueOptionsBuilder RetryPolicy(IRetryPolicy retryPolicy)
        {
            this.Target.RetryPolicy = retryPolicy;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Retries(int retries)
        {
            this.Target.Retries = retries;
            return this;
        }

        public AzureStorageQueueOptionsBuilder ProcessTimeout(TimeSpan timeout)
        {
            this.Target.ProcessTimeout = timeout;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public AzureStorageQueueOptionsBuilder DequeueInterval(TimeSpan dequeueInterval)
        {
            this.Target.DequeueInterval = dequeueInterval;
            return this;
        }
    }
}

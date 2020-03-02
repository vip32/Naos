namespace Naos.Queueing.Infrastructure.Azure
{
    using System;
    using MediatR;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    public class AzureStorageQueueOptionsBuilder :
       BaseOptionsBuilder<AzureStorageQueueOptions, AzureStorageQueueOptionsBuilder>
    {
        public AzureStorageQueueOptionsBuilder Tracer(ITracer tracer)
        {
            this.Target.Tracer = tracer;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public AzureStorageQueueOptionsBuilder QueueName(string queueName)
        {
            this.Target.QueueName = queueName.Safe().ToLower(); // name may only contain lowercase letters, numbers, and hyphens, and must begin with a letter or a number. Each hyphen must be preceded and followed by a non-hyphen character. The name must also be between 3 and 63 characters long.
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

        public AzureStorageQueueOptionsBuilder NoRetries()
        {
            this.Target.Retries = 0;
            return this;
        }

        public AzureStorageQueueOptionsBuilder ProcessInterval(TimeSpan interval)
        {
            this.Target.ProcessInterval = interval;
            return this;
        }

        public AzureStorageQueueOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;
            return this;
        }

        public AzureStorageQueueOptionsBuilder DequeueInterval(TimeSpan interval)
        {
            this.Target.DequeueInterval = interval;
            return this;
        }
    }
}

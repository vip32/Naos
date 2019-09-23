namespace Naos.Queueing.Infrastructure.Azure
{
    using System;
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;
    using Naos.Foundation;

    public class AzureServiceBusQueueOptionsBuilder :
       BaseOptionsBuilder<AzureServiceBusQueueOptions, AzureServiceBusQueueOptionsBuilder>
    {
        public AzureServiceBusQueueOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder Serializer(ISerializer serializer)
        {
            this.Target.Serializer = serializer;

            return this;
        }

        public AzureServiceBusQueueOptionsBuilder ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder Name(string name)
        {
            this.Target.QueueName = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder AutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
        {
            this.Target.AutoDeleteOnIdle = autoDeleteOnIdle;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder MaxSizeInMegabytes(long maxSizeInMegabytes)
        {
            this.Target.MaxSizeInMegabytes = maxSizeInMegabytes;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder RequiresDuplicateDetection(bool requiresDuplicateDetection)
        {
            this.Target.RequiresDuplicateDetection = requiresDuplicateDetection;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder RequiresSession(bool requiresSession)
        {
            this.Target.RequiresSession = requiresSession;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder DefaultMessageTimeToLive(TimeSpan defaultMessageTimeToLive)
        {
            this.Target.DefaultMessageTimeToLive = defaultMessageTimeToLive;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder EnableDeadLetteringOnMessageExpiration(bool enableDeadLetteringOnMessageExpiration)
        {
            this.Target.EnableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder DuplicateDetectionHistoryTimeWindow(TimeSpan duplicateDetectionHistoryTimeWindow)
        {
            this.Target.DuplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder EnableBatchedOperations(bool enableBatchedOperations)
        {
            this.Target.EnableBatchedOperations = enableBatchedOperations;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder IsAnonymousAccessible(bool isAnonymousAccessible)
        {
            this.Target.IsAnonymousAccessible = isAnonymousAccessible;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder SupportOrdering(bool supportOrdering)
        {
            this.Target.SupportOrdering = supportOrdering;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder Status(EntityStatus status)
        {
            this.Target.Status = status;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder ForwardTo(string forwardTo)
        {
            this.Target.ForwardTo = forwardTo ?? throw new ArgumentNullException(nameof(forwardTo));
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder ForwardDeadLetteredMessagesTo(string forwardDeadLetteredMessagesTo)
        {
            this.Target.ForwardDeadLetteredMessagesTo = forwardDeadLetteredMessagesTo ?? throw new ArgumentNullException(nameof(forwardDeadLetteredMessagesTo));
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder EnablePartitioning(bool enablePartitioning)
        {
            this.Target.EnablePartitioning = enablePartitioning;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder UserMetadata(string userMetadata)
        {
            this.Target.UserMetadata = userMetadata ?? throw new ArgumentNullException(nameof(userMetadata));
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder EnableExpress(bool enableExpress)
        {
            this.Target.EnableExpress = enableExpress;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder ProcessInterval(TimeSpan interval)
        {
            this.Target.ProcessInterval = interval;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder RetryPolicy(RetryPolicy retryPolicy)
        {
            this.Target.RetryPolicy = retryPolicy;
            return this;
        }

        public AzureServiceBusQueueOptionsBuilder Retries(int retries)
        {
            this.Target.Retries = retries;
            return this;
        }
    }
}

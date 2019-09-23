namespace Naos.UnitTests.Queueing
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
    using Naos.Queueing.Infrastructure.Azure;
    using NSubstitute;
    using Xunit;

    public class AzureServicebusQueueTests : QueueBaseTests
    {
        private IQueue<StubMessage> queue;

        [Fact]
        public override Task CanQueueAndDequeueAsync()
        {
            return base.CanQueueAndDequeueAsync();
        }

        [Fact]
        public override Task CanDequeueWithCancelledTokenAsync()
        {
            return base.CanDequeueWithCancelledTokenAsync();
        }

        [Fact]
        public override Task CanQueueAndDequeueMultipleAsync()
        {
            return base.CanQueueAndDequeueMultipleAsync();
        }

        [Fact]
        public override Task WillNotWaitForItemAsync()
        {
            return base.WillNotWaitForItemAsync();
        }

        [Fact]
        public override Task WillWaitForItemAsync()
        {
            return base.WillWaitForItemAsync();
        }

        [Fact]
        public override Task DequeueWaitWillGetSignaledAsync()
        {
            return base.DequeueWaitWillGetSignaledAsync();
        }

        [Fact]
        public override Task CanQueueProcessItemsAsync()
        {
            return base.CanQueueProcessItemsAsync();
        }

        [Fact]
        public override Task CanQueueProcessItemsWithMediatorSendAsync()
        {
            return base.CanQueueProcessItemsWithMediatorSendAsync();
        }

        [Fact]
        public override Task CanHandleErrorInProcessItemsAsync()
        {
            return base.CanHandleErrorInProcessItemsAsync();
        }

        [Fact]
        public override Task ItemsWillTimeoutAsync()
        {
            return base.ItemsWillTimeoutAsync();
        }

        [Fact]
        public override Task ItemsWillGetMovedToDeadletterAsync()
        {
            return base.ItemsWillGetMovedToDeadletterAsync();
        }

        [Fact]
        public override Task CanAutoCompleteProcessItemsAsync()
        {
            return base.CanAutoCompleteProcessItemsAsync();
        }

        [Fact]
        public override Task CanDelayRetryAsync()
        {
            return base.CanDelayRetryAsync();
        }

        [Fact]
        public override Task CanRenewLockAsync()
        {
            return base.CanRenewLockAsync();
        }

        [Fact]
        public override Task CanAbandonQueueEntryOnceAsync()
        {
            return base.CanAbandonQueueEntryOnceAsync();
        }

        [Fact]
        public override Task CanCompleteQueueEntryOnceAsync()
        {
            return base.CanCompleteQueueEntryOnceAsync();
        }

        protected override IQueue<StubMessage> GetQueue(
            int retries = 1,
            TimeSpan? processInterval = null,
            TimeSpan? retryDelay = null,
            int deadLetterMaxItems = 100)
        {
            var name = $"test-{Guid.NewGuid().ToString("N").Substring(10)}";
            var connectionString = string.Empty;
            //var connectionString = Configuration["naos:tests:serviceBus:connectionString"];
            if (connectionString.IsNullOrEmpty())
            {
                return null;
            }

            var retryPolicy = retryDelay.GetValueOrDefault() > TimeSpan.Zero
                ? new RetryExponential(retryDelay.GetValueOrDefault(), retryDelay.GetValueOrDefault() + retryDelay.GetValueOrDefault(), retries + 1)
                : RetryPolicy.NoRetry;

            return this.queue ?? (this.queue = new AzureServiceBusQueue<StubMessage>(o => o
                        .Mediator(Substitute.For<IMediator>())
                        .LoggerFactory(Substitute.For<ILoggerFactory>())
                        .ConnectionString(connectionString)
                        .Name(name)
                        .AutoDeleteOnIdle(TimeSpan.FromMinutes(5))
                        .EnableBatchedOperations(true)
                        .EnableExpress(true)
                        .EnablePartitioning(true)
                        .SupportOrdering(false)
                        .RequiresDuplicateDetection(false)
                        .RequiresSession(false)
                        .Retries(retries)
                        .RetryPolicy(retryPolicy)
                        .ProcessInterval(processInterval ?? TimeSpan.FromMinutes(5))));
        }

        protected override Task CleanupQueueAsync(IQueue<StubMessage> queue)
        {
            //queue?.Dispose();
            return Task.CompletedTask;
        }
    }
}

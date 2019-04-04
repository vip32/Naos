namespace Naos.Core.UnitTests.Queueing
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Naos.Core.Common;
    using Naos.Core.Queueing.Domain;
    using Naos.Core.Queueing.Infrastructure.Azure;
    using NSubstitute;
    using Xunit;

    public class AzureStorageQueueTests : QueueBaseTests
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
            TimeSpan? processTimeout = null,
            TimeSpan? retryDelay = null,
            int deadLetterMaxItems = 100)
        {
            var name = $"test-{Guid.NewGuid().ToString("N").Substring(10)}";
            var connectionString = string.Empty;
            if(connectionString.IsNullOrEmpty())
            {
                return null;
            }

            return this.queue ?? (this.queue = new AzureStorageQueue<StubMessage>(o => o
                        .Mediator(Substitute.For<IMediator>())
                        .LoggerFactory(Substitute.For<ILoggerFactory>())
                        .ConnectionString(connectionString)
                        .Name(name)
                        .Retries(retries)
                        .RetryPolicy(retries <= 0 ? new NoRetry() : (IRetryPolicy)new ExponentialRetry(retryDelay.GetValueOrDefault(TimeSpan.FromMinutes(1)), retries))
                        .ProcessTimeout(processTimeout.GetValueOrDefault(TimeSpan.FromMinutes(5)))
                        .DequeueInterval(TimeSpan.FromMilliseconds(100))));
        }

        protected override Task CleanupQueueAsync(IQueue<StubMessage> queue)
        {
            queue?.Dispose();
            return Task.CompletedTask;
        }
    }
}

namespace Naos.UnitTests.Queueing
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing;
    using Naos.Queueing.Domain;
    using NSubstitute;
    using Xunit;

    public class InMemoryQueueTests : QueueBaseTests
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
            return this.queue ?? (this.queue = new InMemoryQueue<StubMessage>(o => o
                        .Mediator(Substitute.For<IMediator>())
                        .LoggerFactory(Substitute.For<ILoggerFactory>())
                        .RetryDelay(retryDelay.GetValueOrDefault(TimeSpan.FromMinutes(1)))
                        .Retries(retries)
                        .ProcessInterval(processInterval ?? TimeSpan.FromMilliseconds(200))
                        .DequeueInterval(TimeSpan.FromMilliseconds(200))));
        }

        protected override async Task CleanupQueueAsync(IQueue<StubMessage> queue)
        {
            await (queue?.DeleteQueueAsync()).AnyContext();
        }
    }
}

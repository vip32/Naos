namespace Naos.Core.UnitTests.Queueing
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Queueing.Domain;
    using Xunit;

    public abstract class QueueBaseTests : IDisposable
    {
        public virtual void Dispose()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            using (queue)
            {
                queue.DeleteQueueAsync().GetAwaiter().GetResult();
            }
        }

        public virtual async Task CanQueueAndDequeueAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });
                Assert.Equal(1, (await queue.GetMetricsAsync()).Enqueued);

                var item = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);
                Assert.Equal(1, (await queue.GetMetricsAsync()).Dequeued);

                await item.CompleteAsync();
                Assert.False(item.IsAbandoned);
                Assert.True(item.IsCompleted);
                var stats = await queue.GetMetricsAsync();
                Assert.Equal(1, stats.Completed);
                Assert.Equal(0, stats.Queued);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        /// <summary>
        /// When a cancelled token is passed into Dequeue, it will only try to dequeue one time and then exit.
        /// </summary>
        /// <returns></returns>
        public virtual async Task CanDequeueWithCancelledTokenAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });
                Assert.Equal(1, (await queue.GetMetricsAsync()).Enqueued);

                var item = await queue.DequeueAsync(new CancellationToken(true));
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);
                Assert.Equal(1, (await queue.GetMetricsAsync()).Dequeued);

                // TODO: We should verify that only one retry occurred.
                await item.CompleteAsync();
                var stats = await queue.GetMetricsAsync();
                Assert.Equal(1, stats.Completed);
                Assert.Equal(0, stats.Queued);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanQueueAndDequeueMultipleAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                const int itemCount = 25;
                for (int i = 0; i < itemCount; i++)
                {
                    await queue.EnqueueAsync(new StubMessage
                    {
                        Text = "Hello"
                    });
                }

                Assert.Equal(itemCount, (await queue.GetMetricsAsync()).Queued);

                for (int i = 0; i < itemCount; i++)
                {
                    var item = await queue.DequeueAsync();
                    Assert.NotNull(item);
                    Assert.Equal("Hello", item.Value.Text);
                    await item.CompleteAsync();
                }

                var stats = await queue.GetMetricsAsync();
                Assert.Equal(itemCount, stats.Dequeued);
                Assert.Equal(itemCount, stats.Completed);
                Assert.Equal(0, stats.Queued);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task WillNotWaitForItemAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                var sw = Stopwatch.StartNew();
                var itemm = await queue.DequeueAsync(TimeSpan.Zero);
                sw.Stop();

                Assert.Null(itemm);
                Assert.InRange(sw.Elapsed.TotalMilliseconds, 0, 200);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task WillWaitForItemAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                var sw = Stopwatch.StartNew();
                var item = await queue.DequeueAsync(TimeSpan.FromMilliseconds(1000)); // wait max 1 sec
                sw.Stop();

                Assert.Null(item);
                Assert.InRange(sw.Elapsed, TimeSpan.FromMilliseconds(999), TimeSpan.FromMilliseconds(5000));

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });

                var sw2 = Stopwatch.StartNew();
                var item2 = await queue.DequeueAsync(TimeSpan.FromMilliseconds(1000)); // don't has to wait as item present in queue
                sw2.Stop();

                Assert.True(sw2.Elapsed < TimeSpan.FromMilliseconds(400));
                Assert.NotNull(item2);
                await item2.CompleteAsync();
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task DequeueWaitWillGetSignaledAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                Thread.Sleep(250);
                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });

                var sw = Stopwatch.StartNew();
                var item = await queue.DequeueAsync(TimeSpan.FromSeconds(2));
                sw.Stop();

                Assert.NotNull(item);
                Assert.InRange(sw.Elapsed.TotalSeconds, 0, 2);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanQueueProcessItemsAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.ProcessItemsAsync(async i =>
                {
                    Assert.Equal("Hello", i.Value.Text);
                    await i.CompleteAsync();
                });

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });
                Thread.Sleep(1000);

                var stats = await queue.GetMetricsAsync();
                Assert.Equal(1, stats.Completed);
                Assert.Equal(0, stats.Queued);
                Assert.Equal(0, stats.Errors);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanHandleErrorInProcessItemsAsync()
        {
            var queue = this.GetQueue(retries: 0);
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.ProcessItemsAsync(i =>
                {
                    Assert.Equal("Hello", i.Value.Text);
                    throw new Exception();
                });

                await queue.EnqueueAsync(new StubMessage { Text = "Hello" });

                Thread.Sleep(1000);
                var stats = await queue.GetMetricsAsync();

                Assert.Equal(0, stats.Completed);
                Assert.Equal(1, stats.Errors);
                Assert.Equal(1, stats.Deadletter);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task ItemsWillTimeoutAsync()
        {
            var queue = this.GetQueue(retryDelay: TimeSpan.Zero, processTimeout: TimeSpan.FromMilliseconds(50));
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });
                var item = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);
                Thread.Sleep(1000);

                // wait for the task to be auto abandoned
                await queue.AbandonAsync(item); // client must abandon

                var sw = Stopwatch.StartNew();
                item = await queue.DequeueAsync(TimeSpan.FromSeconds(5)); // wait 5 secs if no items
                sw.Stop();

                Assert.NotNull(item);
                await item.CompleteAsync();
                Assert.Equal(0, (await queue.GetMetricsAsync()).Queued);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task ItemsWillGetMovedToDeadletterAsync()
        {
            var queue = this.GetQueue(retryDelay: TimeSpan.Zero);
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });
                var item = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.Equal("Hello", item.Value.Text);
                Assert.Equal(1, (await queue.GetMetricsAsync()).Dequeued);

                await item.AbandonAsync();
                Assert.Equal(1, (await queue.GetMetricsAsync()).Abandoned);

                // work item should be retried 1 time.
                item = await queue.DequeueAsync(TimeSpan.FromSeconds(5));
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);
                Assert.Equal(2, (await queue.GetMetricsAsync()).Dequeued);

                await item.AbandonAsync();

                // work item should be moved to deadletter _queue after retries.
                var stats = await queue.GetMetricsAsync();
                Assert.Equal(1, stats.Deadletter);
                Assert.Equal(2, stats.Abandoned);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanAutoCompleteProcessItemsAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.ProcessItemsAsync(i =>
                {
                    Assert.Equal("Hello", i.Value.Text);
                    return Task.CompletedTask;
                }, true);

                await queue.EnqueueAsync(new StubMessage { Text = "Hello" });
                Thread.Sleep(300);

                Assert.Equal(1, (await queue.GetMetricsAsync()).Enqueued);

                var stats = await queue.GetMetricsAsync();
                Assert.Equal(0, stats.Queued);
                Assert.Equal(0, stats.Errors);
                Assert.Equal(1, stats.Completed);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanDelayRetryAsync()
        {
            var queue = this.GetQueue(processTimeout: TimeSpan.FromMilliseconds(500), retryDelay: TimeSpan.FromSeconds(1));
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });

                var item = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);

                var startTime = DateTime.UtcNow;
                await item.AbandonAsync(); // item will be enqueued after 1 second
                Assert.Equal(1, (await queue.GetMetricsAsync()).Abandoned);
                Thread.Sleep(1100); // wait longer than 1 second and try to dequeue again

                item = await queue.DequeueAsync(TimeSpan.FromSeconds(5));
                var elapsed = DateTime.UtcNow.Subtract(startTime);

                Assert.NotNull(item);
                Assert.True(elapsed > TimeSpan.FromSeconds(.95));
                await item.CompleteAsync();
                Assert.Equal(0, (await queue.GetMetricsAsync()).Queued);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanRenewLockAsync()
        {
            // Need large value to reproduce this test
            var timeout = TimeSpan.FromSeconds(1);
            // Slightly shorter than the timeout to ensure we haven't lost the lock
            var renewWait = TimeSpan.FromSeconds(timeout.TotalSeconds * .25d);

            var queue = this.GetQueue(
                retryDelay: TimeSpan.Zero,
                processTimeout: timeout);

            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage
                {
                    Text = "Hello"
                });
                var entry = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.NotNull(entry);
                Assert.Equal("Hello", entry.Value.Text);

                Thread.Sleep(renewWait);
                await entry.RenewLockAsync();

                // We shouldn't get another item here if RenewLock works.
                Thread.Sleep(renewWait);
                var nullItem = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.Null(nullItem);

                await entry.CompleteAsync();
                Assert.Equal(0, (await queue.GetMetricsAsync()).Queued);
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanAbandonQueueEntryOnceAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await this.AssertEmptyQueueAsync(queue);

                await queue.EnqueueAsync(new StubMessage { Text = "Hello" });
                Assert.Equal(1, (await queue.GetMetricsAsync()).Enqueued);

                var item = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);
                Assert.Equal(1, (await queue.GetMetricsAsync()).Dequeued);

                await item.AbandonAsync();
                Assert.True(item.IsAbandoned);
                Assert.False(item.IsCompleted);
                await Assert.ThrowsAnyAsync<Exception>(() => item.AbandonAsync());
                await Assert.ThrowsAnyAsync<Exception>(() => item.CompleteAsync());
                await Assert.ThrowsAnyAsync<Exception>(() => item.CompleteAsync());

                var stats = await queue.GetMetricsAsync();
                Assert.Equal(1, stats.Abandoned);
                Assert.Equal(0, stats.Completed);
                Assert.Equal(0, stats.Deadletter);
                Assert.Equal(1, stats.Dequeued);
                Assert.Equal(1, stats.Enqueued);
                Assert.Equal(0, stats.Errors);
                Assert.InRange(stats.Queued, 0, 1);
                Assert.Equal(0, stats.Timeouts);
                Assert.Equal(0, stats.Working);

                if (item is QueueItem<StubMessage> queueEntry)
                {
                    Assert.Equal(1, queueEntry.Attempts);
                }

                await queue.EnqueueAsync(new StubMessage { Text = "Hello" });
                item = await queue.DequeueAsync(TimeSpan.Zero);

                await queue.AbandonAsync(item);
                Assert.True(item.IsAbandoned);
                Assert.False(item.IsCompleted);
                await Assert.ThrowsAnyAsync<Exception>(() => item.CompleteAsync());
                await Assert.ThrowsAnyAsync<Exception>(() => item.AbandonAsync());
                await Assert.ThrowsAnyAsync<Exception>(() => queue.AbandonAsync(item));
                await Assert.ThrowsAnyAsync<Exception>(() => queue.CompleteAsync(item));
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        public virtual async Task CanCompleteQueueEntryOnceAsync()
        {
            var queue = this.GetQueue();
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
                await queue.EnqueueAsync(new StubMessage { Text = "Hello" });

                Assert.Equal(1, (await queue.GetMetricsAsync()).Enqueued);

                var item = await queue.DequeueAsync(TimeSpan.Zero);
                Assert.NotNull(item);
                Assert.Equal("Hello", item.Value.Text);
                Assert.Equal(1, (await queue.GetMetricsAsync()).Dequeued);

                await item.CompleteAsync();
                await Assert.ThrowsAnyAsync<Exception>(() => item.CompleteAsync());
                await Assert.ThrowsAnyAsync<Exception>(() => item.AbandonAsync());
                await Assert.ThrowsAnyAsync<Exception>(() => item.AbandonAsync());
                var stats = await queue.GetMetricsAsync();
                Assert.Equal(0, stats.Abandoned);
                Assert.Equal(1, stats.Completed);
                Assert.Equal(0, stats.Deadletter);
                Assert.Equal(1, stats.Dequeued);
                Assert.Equal(1, stats.Enqueued);
                Assert.Equal(0, stats.Errors);
                Assert.Equal(0, stats.Queued);
                Assert.Equal(0, stats.Timeouts);
                Assert.Equal(0, stats.Working);

                if (item is QueueItem<StubMessage> queueEntry)
                {
                    Assert.Equal(1, queueEntry.Attempts);
                }
            }
            finally
            {
                await this.CleanupQueueAsync(queue);
            }
        }

        protected virtual IQueue<StubMessage> GetQueue(
            int retries = 1,
            TimeSpan? processTimeout = null,
            TimeSpan? retryDelay = null,
            int deadLetterMaxItems = 100)
        {
            return null;
        }

        protected virtual async Task CleanupQueueAsync(IQueue<StubMessage> queue)
        {
            if (queue == null)
            {
                return;
            }

            try
            {
                await queue.DeleteQueueAsync();
            }
            finally
            {
                queue.Dispose();
            }
        }

        private async Task AssertEmptyQueueAsync(IQueue<StubMessage> queue)
        {
            var stats = await queue.GetMetricsAsync();
            Assert.Equal(0, stats.Abandoned);
            Assert.Equal(0, stats.Completed);
            Assert.Equal(0, stats.Deadletter);
            Assert.Equal(0, stats.Dequeued);
            Assert.Equal(0, stats.Enqueued);
            Assert.Equal(0, stats.Errors);
            Assert.Equal(0, stats.Queued);
            Assert.Equal(0, stats.Timeouts);
            Assert.Equal(0, stats.Working);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class StubMessage
#pragma warning restore SA1402 // File may only contain a single class
    {
        public string Text { get; set; }
    }
}

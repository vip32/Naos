namespace Naos.Core.Common
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Extensions.Logging;

    public static class Run
    {
        private static readonly int[] DefaultBackoffIntervals = new int[] { 100, 1000, 2000, 2000, 5000, 5000, 10000, 30000, 60000 };

        public static Task DelayedAsync(TimeSpan delay, Func<Task> action)
        {
            EnsureArg.IsNotNull(action, nameof(action));

            return Task.Run(() =>
            {
                if (delay.Ticks <= 0)
                {
                    return action();
                }

                return Task.Delay(delay)
                    .ContinueWith(t => action(), TaskContinuationOptions.OnlyOnRanToCompletion);
            });
        }

        public static Task InParallelAsync(int iterations, Func<int, Task> work)
        {
            EnsureArg.IsNotNull(work, nameof(work));

            return Task.WhenAll(Enumerable.Range(1, iterations)
                .Select(i => Task.Run(() => work(i))));
        }

        public static Task WithRetriesAsync(
            Func<Task> action,
            int maxAttempts = 5,
            TimeSpan? retryInterval = null,
            ILogger logger = null,
            CancellationToken cancellationToken = default)
        {
            return WithRetriesAsync(() => action()
                .ContinueWith(t => Task.CompletedTask, TaskContinuationOptions.OnlyOnRanToCompletion), maxAttempts, retryInterval, logger, cancellationToken);
        }

        public static async Task<T> WithRetriesAsync<T>(
            Func<Task<T>> action,
            int maxAttempts = 5,
            TimeSpan? retryInterval = null,
            ILogger logger = null,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(action, nameof(action));

            var attempts = 1;
            var startTime = DateTime.UtcNow;
            var currentBackoffTime = DefaultBackoffIntervals[0];
            if (retryInterval != null)
            {
                currentBackoffTime = (int)retryInterval.Value.TotalMilliseconds;
            }

            do
            {
                if (attempts > 1 && logger != null && logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation($"retry {attempts}/{maxAttempts} attempt after {DateTime.UtcNow.Subtract(startTime).Humanize(3)}...");
                }

                try
                {
                    return await action().AnyContext();
                }
                catch (Exception ex)
                {
                    if (attempts >= maxAttempts)
                    {
                        throw;
                    }

                    if (logger != null && logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(ex, $"retry error: {ex.GetFullMessage()}");
                    }

                    await Task.Delay(currentBackoffTime, cancellationToken).AnyContext();
                }

                if (retryInterval == null)
                {
                    currentBackoffTime = DefaultBackoffIntervals[Math.Min(attempts, DefaultBackoffIntervals.Length - 1)];
                }

                attempts++;
            }
            while (attempts <= maxAttempts && !cancellationToken.IsCancellationRequested);

            throw new TaskCanceledException(); // Should not get here
        }
    }
}
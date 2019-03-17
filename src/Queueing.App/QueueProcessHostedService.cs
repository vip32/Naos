namespace Queueing.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Queueing.Domain;

    public class QueueProcessHostedService<T> : IHostedService, IDisposable
        where T : class
    {
        private readonly ILogger<QueueProcessHostedService<T>> logger;
        private readonly IQueue<T> queue;
        private readonly Func<IQueueItem<T>, Task> handler;

        public QueueProcessHostedService(
            ILogger<QueueProcessHostedService<T>> logger,
            IQueue<T> queue,
            Func<IQueueItem<T>, Task> handler = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = logger;
            this.queue = queue;
            this.handler = handler;
            //handler = i => { i.CompleteAsync()};
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service started", LogEventKeys.Queueing);

            if (this.handler != null)
            {
                await this.queue.ProcessItemsAsync(this.handler);
            }
            else
            {
                await this.queue.ProcessItemsAsync(async i =>
                {
                    this.logger.LogInformation($"+++ process item +++ {i.Id}");
                    await i.CompleteAsync();
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service stopped", LogEventKeys.Queueing);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.queue?.Dispose();
        }
    }
}
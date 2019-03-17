namespace Queueing.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
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
            //await this.queue.ProcessItemsAsync(this.handler);
            await this.queue.ProcessItemsAsync(async i =>
            {
                // do something;
                this.logger.LogInformation("+++ process item +++");
                await i.CompleteAsync();
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.queue?.Dispose();
        }
    }
}
namespace Naos.Queueing.Application.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;

    public class QueueProcessItemsHostedService<TData> : IHostedService, IDisposable
        where TData : class
    {
        private readonly ILogger<QueueProcessItemsHostedService<TData>> logger;
        private readonly IQueue<TData> queue;
        private readonly Func<IQueueItem<TData>, Task> handler;

        public QueueProcessItemsHostedService(
            ILoggerFactory loggerFactory,
            IQueue<TData> queue,
            Func<IQueueItem<TData>, Task> handler = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = loggerFactory.CreateLogger<QueueProcessItemsHostedService<TData>>();
            this.queue = queue;
            this.handler = handler;
            //handler = i => { i.CompleteAsync()};
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service started", LogKeys.Queueing);

            if (this.handler != null)
            {
                await this.queue.ProcessItemsAsync(this.handler, cancellationToken: cancellationToken).AnyContext();
            }
            else
            {
                await this.queue.ProcessItemsAsync(true, cancellationToken).AnyContext();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service stopped", LogKeys.Queueing);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.queue?.Dispose();
        }
    }
}
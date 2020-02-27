namespace Naos.Messaging.Application.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;

    public class QueueProcessingStartupTask<T> : IStartupTask
        where T : class
    {
        private readonly ILogger<QueueProcessingStartupTask<T>> logger;
        private readonly IQueue<T> queue;

        public QueueProcessingStartupTask(ILoggerFactory loggerFactory, IQueue<T> queue)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = loggerFactory.CreateLogger<QueueProcessingStartupTask<T>>();
            this.queue = queue;
        }

        public TimeSpan? Delay { get; set; }

        public bool AutoComplete { get; set; } = true;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await this.queue.ProcessItemsAsync(this.AutoComplete, CancellationToken.None).AnyContext(); // > mediator handler (QueueEvent<>)
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            this.queue.Dispose();
            return Task.CompletedTask;
        }
    }
}

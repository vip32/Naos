namespace Naos.Messaging.Application.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;

    public class QueueProcessItemsStartupTask<T> : IStartupTask
        where T : class
    {
        private readonly ILogger<QueueProcessItemsStartupTask<T>> logger;
        private readonly IQueue<T> queue;

        public QueueProcessItemsStartupTask(ILoggerFactory loggerFactory, IQueue<T> queue)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = loggerFactory.CreateLogger<QueueProcessItemsStartupTask<T>>();
            this.queue = queue;
        }

        public TimeSpan? Delay { get; set; }

        public bool AutoComplete { get; set; } = true; // TODO: set from outside

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await this.queue.ProcessItemsAsync(this.AutoComplete, CancellationToken.None).AnyContext(); // > mediator handler (QueueEvent<>)
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            this.queue?.Dispose();
            return Task.CompletedTask;
        }
    }
}

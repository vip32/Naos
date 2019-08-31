namespace Naos.Foundation.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.App.Web;
    using Naos.Core.Queueing.Domain;

    public class CommandRequestQueueProcessor : IStartupTask
    {
        private readonly ILogger<CommandRequestQueueProcessor> logger;
        private readonly IQueue<CommandWrapper> queue;

        public CommandRequestQueueProcessor(
            ILoggerFactory loggerFactory,
            IQueue<CommandWrapper> queue = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger<CommandRequestQueueProcessor>();
            this.queue = queue;
        }

        public TimeSpan? Delay { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            //this.logger.LogInformation("{LogKey:l} +++ hello from request command queue startup task", LogKeys.AppCommand);
            await this.queue.ProcessItemsAsync(true).AnyContext(); // dequeue items > item is send with mediator > then handled by CommandRequestQueueEventHandler
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

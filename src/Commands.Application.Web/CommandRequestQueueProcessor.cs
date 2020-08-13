namespace Naos.Commands.Application.Web
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;

    public class CommandRequestQueueProcessor : IStartupTask
    {
        private readonly ILogger<CommandRequestQueueProcessor> logger;
        private readonly IQueue<CommandRequestWrapper> queue;

        public CommandRequestQueueProcessor(
            ILoggerFactory loggerFactory,
            IQueue<CommandRequestWrapper> queue = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger<CommandRequestQueueProcessor>();
            this.queue = queue;
        }

        public TimeSpan? Delay { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            //this.logger.LogInformation("{LogKey:l} +++ hello from request command queue startup task", LogKeys.AppCommand);
            await this.queue.ProcessItemsAsync(true, cancellationToken).AnyContext(); // dequeue items > item is send with mediator > then handled by CommandRequestQueueEventHandler
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

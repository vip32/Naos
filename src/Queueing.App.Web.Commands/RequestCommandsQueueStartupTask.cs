namespace Naos.Foundation.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.App.Web;
    using Naos.Core.Queueing.Domain;

    public class RequestCommandsQueueStartupTask : IStartupTask
    {
        private readonly ILogger<RequestCommandsQueueStartupTask> logger;
        private readonly IQueue<CommandRequestWrapper> queue;

        public RequestCommandsQueueStartupTask(
            ILoggerFactory loggerFactory,
            IQueue<CommandRequestWrapper> queue = null)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger<RequestCommandsQueueStartupTask>();
            this.queue = queue;
        }

        public TimeSpan? Delay { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation("{LogKey:l} +++ hello from request command queue startup task", LogKeys.AppCommand);
            await this.queue.ProcessItemsAsync(true).AnyContext(); // dequeue items
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

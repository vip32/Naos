namespace Naos.Core.Queueing.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.App;
    using Naos.Core.Commands.App.Web;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;

    public class QueueDispatcherRequestCommandExtension : CommandRequestBaseExtension
    {
        private readonly ILogger<LoggingCommandRequestExtension> logger;
        private readonly IQueue<CommandRequestWrapper> queue;

        public QueueDispatcherRequestCommandExtension(
            ILogger<LoggingCommandRequestExtension> logger,
            IQueue<CommandRequestWrapper> queue)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = logger;
            this.queue = queue;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command dispatch (name={registration.CommandType?.Name.SliceTill("Command").SliceTill("Query")}, id={command.Id}, type=queue)", LogKeys.AppCommand);

            var wrapper = new CommandRequestWrapper().SetCommand<TCommand, TResponse>(command);
            await this.queue.EnqueueAsync(wrapper).AnyContext();

            var metrics = await this.queue.GetMetricsAsync().AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} request command queue (enqueued=#{metrics.Enqueued}, queued=#{metrics.Queued})", LogKeys.AppCommand);

            // the extension chain is terminated here
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} request command dispatch (name={registration.CommandType?.Name.SliceTill("Command").SliceTill("Query")}, id={command.Id}, type=queue)", LogKeys.AppCommand);

            var wrapper = new CommandRequestWrapper().SetCommand<TCommand>(command);
            await this.queue.EnqueueAsync(wrapper).AnyContext();

            var metrics = await this.queue.GetMetricsAsync().AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} request command queue (enqueued=#{metrics.Enqueued}, queued=#{metrics.Queued})", LogKeys.AppCommand);
            // the extension chain is terminated here
        }
    }
}

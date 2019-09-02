namespace Naos.Core.Queueing.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Commands.App.Web;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;

    /// <summary>
    ///
    ///   HTTP request >
    ///       CommandRequestMiddleware (invoke)
    ///            > ....Extensions
    ///            -----------------------------------------------------------
    ///            > QueueDispatcherCommandRequestExtension
    ///                  > Queue
    ///                      ^ CommandRequestQueueProcessor (listen)
    ///                            > Send QueueEvent<CommandWrapper>
    ///                                 > CommandRequestQueueEventHandler
    ///                                      > Send Command < CommandHandler
    ///
    /// </summary>
    public class QueueDispatcherCommandRequestExtension : CommandRequestBaseExtension
    {
        private readonly ILogger<LoggingCommandRequestExtension> logger;
        private readonly IQueue<CommandWrapper> queue;

        public QueueDispatcherCommandRequestExtension(
            ILogger<LoggingCommandRequestExtension> logger,
            IQueue<CommandWrapper> queue)
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
            this.logger.LogInformation($"{{LogKey:l}} command request dispatch (name={registration.CommandType.PrettyName()}, id={command.Id}, type=queue)", LogKeys.AppCommand);

            //var wrapper = new CommandWrapper().SetCommand<TCommand, TResponse>(command);
            var wrapper = new CommandWrapper().SetCommand(command);
            await this.queue.EnqueueAsync(wrapper).AnyContext();

            var metrics = await this.queue.GetMetricsAsync().AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} request command queue (enqueued=#{metrics.Enqueued}, queued=#{metrics.Queued})", LogKeys.AppCommand);
            await context.Response.Location($"api/queueing/commands/{command.Id}").AnyContext();
            await context.Response.Header("x-commandid", command.Id).AnyContext();
            // the extension chain is terminated here
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            RequestCommandRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} command request dispatch (name={registration.CommandType.PrettyName()}, id={command.Id}, type=queue)", LogKeys.AppCommand);

            // TODO: start queue TRACER
            //var wrapper = new CommandWrapper().SetCommand<TCommand>(command);
            var wrapper = new CommandWrapper().SetCommand(command);
            await this.queue.EnqueueAsync(wrapper).AnyContext();

            var metrics = await this.queue.GetMetricsAsync().AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} request command queue (enqueued=#{metrics.Enqueued}, queued=#{metrics.Queued})", LogKeys.AppCommand);
            await context.Response.Location($"api/queueing/commands/{command.Id}").AnyContext();
            await context.Response.Header("x-commandid", command.Id).AnyContext();

            // the extension chain is terminated here
        }
    }
}

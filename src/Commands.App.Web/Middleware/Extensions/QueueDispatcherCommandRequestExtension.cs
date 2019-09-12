namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
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
    public class QueueDispatcherCommandRequestExtension : CommandRequestExtension
    {
        private readonly ILogger<QueueDispatcherCommandRequestExtension> logger;
        private readonly IQueue<CommandRequestWrapper> queue;
        private readonly CommandRequestStore storage;

        public QueueDispatcherCommandRequestExtension(
            ILogger<QueueDispatcherCommandRequestExtension> logger,
            IQueue<CommandRequestWrapper> queue,
            CommandRequestStore storage = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(queue, nameof(queue));

            this.logger = logger;
            this.queue = queue;
            this.storage = storage;
        }

        public override async Task InvokeAsync<TCommand, TResponse>(
            TCommand command,
            CommandRequestRegistration<TCommand, TResponse> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} command request dispatch (name={registration.CommandType.PrettyName()}, id={command.Id}, type=queue)", LogKeys.AppCommand);

            //var wrapper = new CommandWrapper().SetCommand<TCommand, TResponse>(command);
            var wrapper = new CommandRequestWrapper().SetCommand(command);
            //wrapper.ParentSpanId = command.Properties.GetValueOrDefault("ParentSpanId") as string; // TODO: no magic string

            await this.queue.EnqueueAsync(wrapper).AnyContext();

            var metrics = await this.queue.GetMetricsAsync().AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} request command queue (enqueued=#{metrics.Enqueued}, queued=#{metrics.Queued})", LogKeys.AppCommand);

            await context.Response.Location($"api/commands/{command.Id}/response").AnyContext();
            await context.Response.Header(CommandRequestHeaders.CommandId, command.Id).AnyContext();
            await this.StoreCommand(wrapper).AnyContext();

            // the extension chain is terminated here
        }

        public override async Task InvokeAsync<TCommand>(
            TCommand command,
            CommandRequestRegistration<TCommand> registration,
            HttpContext context)
        {
            this.logger.LogInformation($"{{LogKey:l}} command request dispatch (name={registration.CommandType.PrettyName()}, id={command.Id}, type=queue)", LogKeys.AppCommand);

            //var wrapper = new CommandWrapper().SetCommand<TCommand>(command);
            var wrapper = new CommandRequestWrapper().SetCommand(command);
            await this.queue.EnqueueAsync(wrapper).AnyContext();

            var metrics = await this.queue.GetMetricsAsync().AnyContext();
            this.logger.LogInformation($"{{LogKey:l}} request command queue (enqueued=#{metrics.Enqueued}, queued=#{metrics.Queued})", LogKeys.AppCommand);

            await context.Response.Location($"api/commands/{command.Id}/response").AnyContext();
            await context.Response.Header(CommandRequestHeaders.CommandId, command.Id).AnyContext();
            await this.StoreCommand(wrapper).AnyContext();

            // the extension chain is terminated here
        }

        private async Task StoreCommand(CommandRequestWrapper wrapper)
        {
            if (this.storage != null)
            {
                // optionaly store the command/response so it can later be retrieved by the client (because the command was queued with no direct response)
                await this.storage.SaveAsync(wrapper).AnyContext();
            }
        }
    }
}

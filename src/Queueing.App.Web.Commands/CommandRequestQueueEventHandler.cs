namespace Naos.Core.Queueing.App.Web
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;

    public class CommandRequestQueueEventHandler : QueueEventHandler<CommandWrapper>
    {
        private readonly ILogger<CommandRequestQueueEventHandler> logger;

        public CommandRequestQueueEventHandler(ILogger<CommandRequestQueueEventHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> Handle(QueueEvent<CommandWrapper> request, CancellationToken cancellationToken)
        {
            if (request?.Item?.Data?.Command != null)
            {
                await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} command request dequeued (name={request.Item.Data.Command.GetType()?.Name.SliceTill("Command").SliceTill("Query")}, id={request.Item?.Id}, type=queue)", LogKeys.AppCommand)).AnyContext();

                // TODO: unwrap the command and send with mediator > actual command handler will then handle it
                // OPTIONAL: get response and store somewhere (repo/filestorage), then for async commands the response can be retrieved by client
            }

            return true;
        }
    }
}

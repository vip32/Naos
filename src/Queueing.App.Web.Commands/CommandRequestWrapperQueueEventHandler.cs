namespace Naos.Core.Queueing.App.Web
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;

    public class CommandRequestWrapperQueueEventHandler : QueueEventHandler<CommandRequestWrapper>
    {
        private readonly ILogger<CommandRequestWrapperQueueEventHandler> logger;

        public CommandRequestWrapperQueueEventHandler(ILogger<CommandRequestWrapperQueueEventHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> Handle(QueueEvent<CommandRequestWrapper> request, CancellationToken cancellationToken)
        {
            if (request?.Item?.Data?.Command != null)
            {
                await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} request command dequeued (name={request.Item.Data.Command.GetType()?.Name.SliceTill("Command").SliceTill("Query")}, id={request.Item?.Id}, type=queue)", LogKeys.AppCommand)).AnyContext();
            }

            return true;
        }
    }
}

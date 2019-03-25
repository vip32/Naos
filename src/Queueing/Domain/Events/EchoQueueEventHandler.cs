namespace Naos.Core.Queueing.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class EchoQueueEventHandler : QueueEventHandler<EchoQueueEventData>
    {
        private readonly ILogger<EchoQueueEventHandler> logger;

        public EchoQueueEventHandler(ILogger<EchoQueueEventHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> Handle(QueueEvent<EchoQueueEventData> request, CancellationToken cancellationToken)
        {
            await Task.Run(() => this.logger.LogInformation($"{{LogKey}} {request.Item.Data.Message} (id={request.Item.Id}, type={this.GetType().PrettyName()})", LogEventKeys.Queueing));
            return true;
        }
    }
}

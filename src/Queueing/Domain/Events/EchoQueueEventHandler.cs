namespace Naos.Core.Queueing.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class EchoQueueEventHandler : QueueEventHandler<EchoQueueEventData>
    {
        private readonly ILogger<EchoQueueEventHandler> logger;

        public EchoQueueEventHandler(ILogger<EchoQueueEventHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> Handle(QueueEvent<EchoQueueEventData> request, CancellationToken cancellationToken)
        {
            await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} {request.Item.Data.Text} (id={request.Item.Id}, type={this.GetType().PrettyName()})", LogKeys.Queueing)).AnyContext();
            return true;
        }
    }
}

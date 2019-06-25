namespace Naos.Core.JobScheduling.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class EchoJobEventHandler : JobEventHandler<EchoJobEventData>
    {
        private readonly ILogger<EchoJobEventHandler> logger;

        public EchoJobEventHandler(ILoggerFactory loggerFactory)
        {
            EnsureThat.EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger<EchoJobEventHandler>();
        }

        public override async Task<bool> Handle(JobEvent<EchoJobEventData> request, CancellationToken cancellationToken)
        {
            await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} {request?.Data?.Text} (type={this.GetType().PrettyName()})", LogKeys.JobScheduling));
            return true;
        }
    }
}

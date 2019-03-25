namespace Naos.Core.JobScheduling.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

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
            await Task.Run(() => this.logger.LogInformation($"{{LogKey}} {request?.Data?.Text} (type={this.GetType().PrettyName()})", LogEventKeys.JobScheduling));
            return true;
        }
    }
}

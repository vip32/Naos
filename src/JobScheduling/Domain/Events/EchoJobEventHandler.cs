namespace Naos.Core.JobScheduling.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Tracing.Domain;
    using Naos.Foundation;

    public class EchoJobEventHandler : JobEventHandler<EchoJobEventData>
    {
        private readonly ILogger<EchoJobEventHandler> logger;
        private readonly ITracer tracer;

        public EchoJobEventHandler(ILoggerFactory loggerFactory, ITracer tracer)
        {
            EnsureThat.EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureThat.EnsureArg.IsNotNull(tracer, nameof(tracer));

            this.logger = loggerFactory.CreateLogger<EchoJobEventHandler>();
            this.tracer = tracer;
        }

        public override async Task<bool> Handle(JobEvent<EchoJobEventData> request, CancellationToken cancellationToken)
        {
            using(var scope = this.tracer.BuildSpan(this.GetType().Name.ToLower()).Activate())
            {
               await Run.DelayedAsync(
                   new System.TimeSpan(0, 0, 3),
                   async () =>
                        await Task.Run(() => this.logger.LogInformation($"{{LogKey:l}} {request?.Data?.Text} (type={this.GetType().PrettyName()})", LogKeys.JobScheduling))).AnyContext();
            }

            return true;
        }
    }
}

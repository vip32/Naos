namespace Naos.Foundation.Application
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class EchoStartupTask : IStartupTask
    {
        private readonly ILogger<EchoStartupTask> logger;

        public EchoStartupTask(ILoggerFactory loggerFactory)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger<EchoStartupTask>();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation("{LogKey:l} +++ hello from echo startup task", LogKeys.StartupTask);
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

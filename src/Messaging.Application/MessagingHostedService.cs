namespace Naos.Messaging.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class MessagingHostedService : IHostedService, IDisposable // TODO: or use BackgroundService? https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/background-tasks-with-ihostedservice#implementing-ihostedservice-with-a-custom-hosted-service-class-deriving-from-the-backgroundservice-base-class
    {
        private readonly ILogger<MessagingHostedService> logger;
        private readonly IMessageBroker broker;

        public MessagingHostedService(
            ILogger<MessagingHostedService> logger,
            IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.logger = logger;
            this.broker = serviceProvider.GetRequiredService<IMessageBroker>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service started", LogKeys.Messaging);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service stopped", LogKeys.Messaging);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //this.broker?.Dispose();
        }
    }
}

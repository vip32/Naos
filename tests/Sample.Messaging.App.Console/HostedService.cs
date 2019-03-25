namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Queueing;
    using Naos.Core.Queueing.Domain;

    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> logger;
        private readonly IServiceProvider serviceProvider;
        private IQueue<EchoQueueEventData> queue;
        private IMessageBroker messageBus;

        public HostedService(ILogger<HostedService> logger, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("starting hosted service");

            this.queue = new InMemoryQueue<EchoQueueEventData>(o => o
                    .Mediator(this.serviceProvider.GetRequiredService<IMediator>())
                    .LoggerFactory(this.serviceProvider.GetRequiredService<ILoggerFactory>()));

            this.messageBus = this.serviceProvider.GetRequiredService<IMessageBroker>()
                .Subscribe<TestMessage, TestMessageHandler>()
                .Subscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            while (true)
            {
                Console.WriteLine("ready to publish?");
                Console.ReadLine();

                await this.PublishAsync().AnyContext();
            }

            // Wait
            //WaitHandle.WaitOne();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("stopping hosted service");
            //this.messageBus.Unsubscribe<TestMessage, TestMessageHandler>();
            //this.messageBus.Unsubscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            //Serilog.Log.CloseAndFlush();

            return Task.CompletedTask;
        }

        private async Task PublishAsync()
        {
            Console.WriteLine("start publish");

            for (int i = 1; i <= 2; i++)
            {
                //Thread.Sleep(500);
                this.messageBus.Publish(new TestMessage { Id = RandomGenerator.GenerateString(7, true), Data = $"{i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}" });
                this.messageBus.Publish(new EntityMessage<StubEntity> { Id = RandomGenerator.GenerateString(7, true), Entity = new StubEntity { FirstName = "John", LastName = $"{RandomGenerator.GenerateString(3, false).ToUpper()} ({i})" } });

                await this.queue.EnqueueAsync(new EchoQueueEventData { Message = "+++ hello from queue item +++" }).AnyContext();
                var metrics = this.queue.GetMetricsAsync().Result;
                Console.WriteLine(metrics.Dump());
            }

            await this.queue.ProcessItemsAsync(true).AnyContext();
        }
    }
}

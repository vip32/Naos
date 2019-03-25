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
    using Naos.Core.JobScheduling.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Queueing;
    using Naos.Core.Queueing.Domain;

    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> logger;
        private readonly IServiceProvider serviceProvider;
        private IQueue<EchoQueueEventData> queue;
        private IJobScheduler jobScheduler;
        private IMessageBroker messageBroker;

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

            this.jobScheduler = this.serviceProvider.GetRequiredService<IJobScheduler>();

            this.messageBroker = this.serviceProvider.GetRequiredService<IMessageBroker>()
                .Subscribe<EchoMessage, EchoMessageHandler>()
                .Subscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            this.queue = new InMemoryQueue<EchoQueueEventData>(o => o
                    .Mediator(this.serviceProvider.GetRequiredService<IMediator>())
                    .LoggerFactory(this.serviceProvider.GetRequiredService<ILoggerFactory>()));
            await this.queue.ProcessItemsAsync(true).AnyContext();

            while (true)
            {
                Thread.Sleep(500);
                Console.WriteLine("\r\nready to publish & queue & start job?");
                Console.ReadLine();

                await this.PublishAsync().AnyContext();
                Thread.Sleep(500);
                await this.EnqueueAsync().AnyContext();
                Thread.Sleep(500);
                await this.TriggerJobAsync().AnyContext();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("stopping hosted service");
            //this.messageBus.Unsubscribe<TestMessage, TestMessageHandler>();
            //this.messageBus.Unsubscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            return Task.CompletedTask;
        }

        private Task PublishAsync()
        {
            Console.WriteLine("\r\n--- start publish");

            for (int i = 1; i <= 2; i++)
            {
                //Thread.Sleep(500);
                this.messageBroker.Publish(new EchoMessage { Text = $"+++ hello from echo message ({i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}) +++" });
                this.messageBroker.Publish(new EntityMessage<StubEntity> { Id = RandomGenerator.GenerateString(7, true), Entity = new StubEntity { FirstName = "John", LastName = $"{RandomGenerator.GenerateString(3, false).ToUpper()} ({i})" } });
            }

            return Task.CompletedTask;
        }

        private async Task EnqueueAsync()
        {
            Console.WriteLine("\r\n--- start enqueue");

            for (int i = 1; i <= 3; i++)
            {
                await this.queue.EnqueueAsync(new EchoQueueEventData { Text = "+++ hello from queue item +++" }).AnyContext();
                var metrics = this.queue.GetMetricsAsync().Result;
                Console.WriteLine(metrics.Dump());
            }
        }

        private async Task TriggerJobAsync()
        {
            Console.WriteLine("\r\n--- start job");

            await this.jobScheduler.TriggerAsync("testjob1").AnyContext();
        }
    }
}

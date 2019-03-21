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
    using Naos.Core.Queueing;
    using Naos.Core.Queueing.Domain;

    public class MessagingTestHostedService : IHostedService
    {
        private readonly ILogger<MessagingTestHostedService> logger;
        private readonly IServiceProvider serviceProvider;
        private IQueue<TestData> queue;
        private IMessageBroker messageBus;

        public MessagingTestHostedService(ILogger<MessagingTestHostedService> logger, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("starting hosted service");

            this.queue = new InMemoryQueue<TestData>(
                new InMemoryQueueOptionsBuilder()
                    .Mediator(this.serviceProvider.GetRequiredService<IMediator>())
                    .LoggerFactory(this.serviceProvider.GetRequiredService<ILoggerFactory>()).Build());

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

                await this.queue.EnqueueAsync(new TestData { FirstName = "John", LastName = "Doe" }).AnyContext();
                var metrics = this.queue.GetMetricsAsync().Result;
                Console.WriteLine(metrics.Dump());
            }

            await this.queue.ProcessItemsAsync(true).AnyContext();
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class TestData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class TestDataRequestHandler : BaseQueueItemRequestHandler<QueueItemRequest<TestData>, TestData>
    {
        private readonly ILogger<TestDataRequestHandler> logger;

        public TestDataRequestHandler(ILogger<TestDataRequestHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> Handle(QueueItemRequest<TestData> request, CancellationToken cancellationToken)
        {
            await Task.Run(() => this.logger.LogInformation($"{{LogKey}} handle (id={request.Item.Id}, type={this.GetType().PrettyName()})", LogEventKeys.Queueing));
            return true;
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}

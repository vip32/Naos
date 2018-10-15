namespace Naos.Core.Sample.Messaging.App.Console
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;

    public class PublishHostedService : IHostedService
    {
        private readonly ILogger<PublishHostedService> logger;
        private readonly IMessageBus eventBus;

        public PublishHostedService(ILogger<PublishHostedService> logger, IMessageBus eventBus)
        {
            this.logger = logger;
            this.eventBus = eventBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            System.Console.WriteLine("starting hosted service");

            // subscribe
            this.eventBus.Subscribe<TestMessage, TestMessageHandler>();
            //this.eventBus.Subscribe<EntityIntegrationEvent<StubEntity>, EntityIntegrationEventHandler<StubEntity>>();
            this.eventBus.Subscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            Task.Run(() =>
            {
                while (true)
                {
                    System.Console.WriteLine("ready to publish?");
                    System.Console.ReadLine();

                    this.Publish();
                }
            });

            // Wait
            //WaitHandle.WaitOne();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            System.Console.WriteLine("stopping hosted service");
            //this.eventBus.Unsubscribe<TestIntegrationEvent, TestIntegrationEventHandler>();

            return Task.CompletedTask;
        }

        private void Publish()
        {
            System.Console.WriteLine("start publish");

            // publish
            for (int i = 1; i <= 2; i++)
            {
                //Thread.Sleep(500);
                this.eventBus.Publish(new TestMessage { Data = $"{i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}" });
                this.eventBus.Publish(new EntityMessage<StubEntity> { Entity = new StubEntity { FirstName = $"John", LastName = $"{RandomGenerator.GenerateString(3, false).ToUpper()} ({i})" } });
            }
        }
    }
}

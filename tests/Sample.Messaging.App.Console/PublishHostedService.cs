namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Common;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using SimpleInjector;

    public class PublishHostedService : IHostedService
    {
        //private readonly ILogger<PublishHostedService> logger;
        private readonly Container container = new Container();
        private IMessageBus messageBus;

        //public PublishHostedService(ILogger<PublishHostedService> logger, IMessageBus messageBus)
        //{
        //    this.logger = logger;
        //    this.messageBus = messageBus;
        //}

        public Task StartAsync(CancellationToken cancellationToken)
        {
            System.Console.WriteLine("starting hosted service");

            var configuration = NaosConfigurationFactory.CreateRoot();
            string[] capabilities = { $"{AppDomain.CurrentDomain.FriendlyName}-A", $"{AppDomain.CurrentDomain.FriendlyName}-B", $"{AppDomain.CurrentDomain.FriendlyName}-C" };
            this.container
                .BuildNaosOperations()
                .BuildNaosMessaging(
                    configuration,
                    subscriptionName: capabilities[new Random().Next(0, capabilities.Length)],
                    assemblies: new[] { typeof(StubEntityMessageHandler).Assembly });

            this.messageBus = this.container.GetInstance<IMessageBus>();

            // subscribe
            this.messageBus.Subscribe<TestMessage, TestMessageHandler>();
            //this.messageBus.Subscribe<EntityIntegrationEvent<StubEntity>, EntityIntegrationEventHandler<StubEntity>>();
            this.messageBus.Subscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

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
                this.messageBus.Publish(new TestMessage { Data = $"{i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}" });
                this.messageBus.Publish(new EntityMessage<StubEntity> { Entity = new StubEntity { FirstName = $"John", LastName = $"{RandomGenerator.GenerateString(3, false).ToUpper()} ({i})" } });
            }
        }
    }
}

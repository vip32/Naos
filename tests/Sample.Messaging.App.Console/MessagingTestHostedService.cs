namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;
    using SimpleInjector;

    public class MessagingTestHostedService : IHostedService
    {
        private readonly Container container = new Container();
        private readonly IServiceProvider serviceProvider;
        private IMessageBroker messageBus;
        private ILogger<MessagingTestHostedService> logger;

        public MessagingTestHostedService(ILogger<MessagingTestHostedService> logger, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("starting hosted service");

            this.messageBus = this.serviceProvider.GetRequiredService<IMessageBroker>();

            // subscribe
            this.messageBus.Subscribe<TestMessage, TestMessageHandler>();
            this.messageBus.Subscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine("ready to publish?");
                    Console.ReadLine();

                    this.Publish();
                }
            });

            // Wait
            //WaitHandle.WaitOne();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("stopping hosted service");
            //this.messageBus.Unsubscribe<TestMessage, TestMessageHandler>();
            //this.messageBus.Unsubscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            //Serilog.Log.CloseAndFlush();

            return Task.CompletedTask;
        }

        private void Publish()
        {
            Console.WriteLine("start publish");

            for (int i = 1; i <= 2; i++)
            {
                //Thread.Sleep(500);
                this.messageBus.Publish(new TestMessage { Data = $"{i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}" });
                this.messageBus.Publish(new EntityMessage<StubEntity> { Entity = new StubEntity { FirstName = $"John", LastName = $"{RandomGenerator.GenerateString(3, false).ToUpper()} ({i})" } });
            }
        }
    }
}

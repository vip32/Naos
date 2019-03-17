﻿namespace Naos.Core.Sample.Messaging.App.Console
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
    using Naos.Core.Queueing;
    using Naos.Core.Queueing.Domain;

    public class MessagingTestHostedService : IHostedService
    {
        private readonly ILogger<MessagingTestHostedService> logger;
        private readonly IServiceProvider serviceProvider;
        private IQueue<TestQueueItem> queue;
        private IMessageBroker messageBus;

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

            this.queue = new InMemoryQueue<TestQueueItem>(
                new InMemoryQueueOptionsBuilder().LoggerFactory(this.serviceProvider.GetRequiredService<ILoggerFactory>()).Build());

            this.messageBus = this.serviceProvider.GetRequiredService<IMessageBroker>()
                .Subscribe<TestMessage, TestMessageHandler>()
                .Subscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

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
                this.messageBus.Publish(new TestMessage { Id = RandomGenerator.GenerateString(7, true), Data = $"{i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}" });
                this.messageBus.Publish(new EntityMessage<StubEntity> { Id = RandomGenerator.GenerateString(7, true), Entity = new StubEntity { FirstName = "John", LastName = $"{RandomGenerator.GenerateString(3, false).ToUpper()} ({i})" } });

                this.queue.EnqueueAsync(new TestQueueItem { FirstName = "John", LastName = "Doe" });
                var metrics = this.queue.GetMetricsAsync().Result;
                Console.WriteLine(metrics.Dump());
            }
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class TestQueueItem
#pragma warning restore SA1402 // File may only contain a single class
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}

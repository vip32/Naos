namespace Naos.Sample.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;
    using Naos.Core.Domain;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain;
    using Naos.Core.Queueing;
    using Naos.Core.Queueing.Domain;
    using Console = Colorful.Console;

    public class ConsoleHostedService : IHostedService
    {
        private readonly ILogger<ConsoleHostedService> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IEnumerable<IConsoleCommand> commands;
        private readonly IMediator mediator;
        private IMessageBroker messageBroker;
        private IQueue<EchoQueueEventData> queue;
        private IJobScheduler jobScheduler;

        public ConsoleHostedService(ILogger<ConsoleHostedService> logger, IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            this.logger = logger;
            this.serviceProvider = serviceProvider;

            this.commands = this.serviceProvider.GetServices<IConsoleCommand>().Distinct();
            this.mediator = (IMediator)this.serviceProvider.CreateScope().ServiceProvider.GetService(typeof(IMediator));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("\r\n--- naos console start", Color.LimeGreen);

            foreach(var command in this.commands.Safe())
            {
                Console.WriteLine("found command: " + command.GetType(), Color.Gray);
                // TODO: parse line > command
                await command.SendAsync(this.mediator).AnyContext();
            }

            this.jobScheduler = this.serviceProvider.GetRequiredService<IJobScheduler>();

            this.messageBroker = this.serviceProvider.GetRequiredService<IMessageBroker>()
                .Subscribe<EchoMessage, EchoMessageHandler>()
                .Subscribe<EntityMessage<EchoEntity>, EchoEntityMessageHandler>();

            this.queue = new InMemoryQueue<EchoQueueEventData>(o => o
                    .Mediator(this.mediator)
                    .LoggerFactory(this.serviceProvider.GetRequiredService<ILoggerFactory>()));
            await this.queue.ProcessItemsAsync(true).AnyContext();

            Thread.Sleep(500);
            while (true)
            {
                Thread.Sleep(500);
                Console.WriteLine("\r\nready to publish & queue & start job?", Color.LimeGreen);
                Console.ReadLine(); // https://github.com/tonerdo/readline

                await this.PublishAsync().AnyContext();
                Thread.Sleep(500);
                await this.EnqueueAsync().AnyContext();
                Thread.Sleep(500);
                await this.TriggerJobAsync().AnyContext();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("hosted service stopping", Color.Gray);
            //this.messageBus.Unsubscribe<TestMessage, TestMessageHandler>();
            //this.messageBus.Unsubscribe<EntityMessage<StubEntity>, StubEntityMessageHandler>();

            return Task.CompletedTask;
        }

        private Task PublishAsync()
        {
            Console.WriteLine("\r\n--- start publish", Color.LimeGreen);

            for (int i = 1; i <= 2; i++)
            {
                //Thread.Sleep(500);
                this.messageBroker.Publish(new EchoMessage { Text = $"+++ hello from echo message ({i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}) +++" });
                this.messageBroker.Publish(new EntityMessage<EchoEntity> { Entity = new EchoEntity { Text = $"+++ hello from echo entity message ({i}-{RandomGenerator.GenerateString(3, false).ToUpper()} +++" } });
            }

            return Task.CompletedTask;
        }

        private async Task EnqueueAsync()
        {
            Console.WriteLine("\r\n--- start enqueue", Color.LimeGreen);

            for (int i = 1; i <= 2; i++)
            {
                await this.queue.EnqueueAsync(new EchoQueueEventData { Text = "+++ hello from queue item +++" }).AnyContext();
                var metrics = this.queue.GetMetricsAsync().Result;
                Console.WriteLine(metrics.Dump());
            }
        }

        private async Task TriggerJobAsync()
        {
            Console.WriteLine("\r\n--- start job", Color.LimeGreen);

            await this.jobScheduler.TriggerAsync("testjob1").AnyContext();
        }
    }
}

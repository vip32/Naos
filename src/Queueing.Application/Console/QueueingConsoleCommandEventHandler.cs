namespace Naos.Queueing.Application
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Queueing.Domain;
    using Console = Colorful.Console;

    public class QueueingConsoleCommandEventHandler : ConsoleCommandEventHandler<QueueingConsoleCommand>, IDisposable
    {
        private readonly IQueue<EchoQueueEventData> queue;
        private readonly ILogger<QueueingConsoleCommandEventHandler> logger;
        private readonly IMediator mediator;

        public QueueingConsoleCommandEventHandler(ILoggerFactory loggerFactory, IMediator mediator, IQueue<EchoQueueEventData> queue)
        {
            this.logger = loggerFactory.CreateLogger<QueueingConsoleCommandEventHandler>();
            this.mediator = mediator;

            if (queue == null)
            {
                Console.WriteLine("\r\ncreate new inmemory queue", Color.LimeGreen);
                this.queue = new InMemoryQueue<EchoQueueEventData>(o => o
                    .Mediator(this.mediator)
                    .LoggerFactory(loggerFactory));
                //await Queue.ProcessItemsAsync(true).AnyContext();
            }
            else
            {
                this.queue = queue;
            }
        }

        public void Dispose()
        {
            if (this.queue?.GetType() == typeof(InMemoryQueue<EchoQueueEventData>))
            {
                this.queue.Dispose();
            }
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<QueueingConsoleCommand> request, CancellationToken cancellationToken)
        {
            if (request.Command.Echo)
            {
                await this.queue.ProcessItemsAsync(true, cancellationToken).AnyContext();

                for (var i = 1; i <= 5; i++)
                {
                    await this.queue.EnqueueAsync(new EchoQueueEventData { Text = $"+++ hello from queue item {i} +++" }).AnyContext();
                    //var metrics = this.queue.GetMetricsAsync().Result;
                    //Console.WriteLine(metrics.Dump());
                }
            }

            return true;
        }
    }
}

namespace Naos.Core.Queueing.App
{
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Queueing.Domain;
    using Naos.Foundation;
    using Console = Colorful.Console;

    public class QueueingConsoleCommandEventHandler : ConsoleCommandEventHandler<QueueingConsoleCommand>
    {
        private static IQueue<EchoQueueEventData> queue;
        private readonly ILogger<QueueingConsoleCommandEventHandler> logger;
        private readonly IMediator mediator;

        public QueueingConsoleCommandEventHandler(ILoggerFactory loggerFactory, IMediator mediator)
        {
            this.logger = loggerFactory.CreateLogger<QueueingConsoleCommandEventHandler>();
            this.mediator = mediator;

            if(queue == null)
            {
                Console.WriteLine("\r\ncreate new inmemory queue", Color.LimeGreen);
                queue = new InMemoryQueue<EchoQueueEventData>(o => o
                    .Mediator(this.mediator)
                    .LoggerFactory(loggerFactory));
                //await Queue.ProcessItemsAsync(true).AnyContext();
            }
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<QueueingConsoleCommand> request, CancellationToken cancellationToken)
        {
            if(request.Command.Echo)
            {
                await queue.ProcessItemsAsync(true).AnyContext();

                for(var i = 1; i <= 2; i++)
                {
                    await queue.EnqueueAsync(new EchoQueueEventData { Text = $"+++ hello from queue item {i} +++" }).AnyContext();
                    var metrics = queue.GetMetricsAsync().Result;
                    Console.WriteLine(metrics.Dump());
                }
            }

            return true;
        }
    }
}

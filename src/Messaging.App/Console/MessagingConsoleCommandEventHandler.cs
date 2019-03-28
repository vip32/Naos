namespace Naos.Core.Messaging.App
{
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Console;
    using Naos.Core.Domain;
    using Naos.Core.Messaging.Domain;
    using Console = Colorful.Console;

    public class MessagingConsoleCommandEventHandler : ConsoleCommandEventHandler<MessagingConsoleCommand>
    {
        private readonly ILogger<MessagingConsoleCommandEventHandler> logger;
        private readonly IMessageBroker messageBroker;

        public MessagingConsoleCommandEventHandler(ILogger<MessagingConsoleCommandEventHandler> logger, IMessageBroker messageBroker)
        {
            this.logger = logger;
            this.messageBroker = messageBroker;
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<MessagingConsoleCommand> request, CancellationToken cancellationToken)
        {
            if (request.Command.Echo)
            {
                this.messageBroker
                    .Subscribe<EchoMessage, EchoMessageHandler>()
                    .Subscribe<EntityMessage<EchoEntity>, EchoEntityMessageHandler>();

                Console.WriteLine("\r\nstart publish", Color.LimeGreen);

                await Task.Run(() =>
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        //Thread.Sleep(500);
                        this.messageBroker.Publish(new EchoMessage { Text = $"+++ hello from echo message ({i.ToString()}-{RandomGenerator.GenerateString(3, false).ToUpper()}) +++" });
                        this.messageBroker.Publish(new EntityMessage<EchoEntity> { Entity = new EchoEntity { Text = $"+++ hello from echo entity message ({i}-{RandomGenerator.GenerateString(3, false).ToUpper()} +++" } });
                    }
                });
            }

            return true;
        }
    }
}

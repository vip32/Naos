namespace Naos.Core.Common.Console
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Console = Colorful.Console;

    public class EchoConsoleCommandEventHandler : ConsoleCommandEventHandler<EchoConsoleCommand>
    {
        private readonly ILogger<EchoConsoleCommandEventHandler> logger;

        public EchoConsoleCommandEventHandler(ILogger<EchoConsoleCommandEventHandler> logger)
        {
            this.logger = logger;
        }

        public override Task<bool> Handle(ConsoleCommandEvent<EchoConsoleCommand> request, CancellationToken cancellationToken)
        {
            var text = request.Command.Text ?? "+++ hello from echo console command";
            if (request.Command.Timestamp)
            {
                text += $" {DateTime.UtcNow.ToEpoch()}";
            }

            //this.logger.LogInformation(text);
            Console.WriteLine(text, Color.Gray);
            return Task.FromResult(true);
        }
    }
}

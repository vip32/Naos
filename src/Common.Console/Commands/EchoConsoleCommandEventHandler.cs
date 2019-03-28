namespace Naos.Core.Common.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class EchoConsoleCommandEventHandler : ConsoleCommandEventHandler<EchoConsoleCommand>
    {
        private readonly ILogger<EchoConsoleCommandEventHandler> logger;

        public EchoConsoleCommandEventHandler(ILogger<EchoConsoleCommandEventHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task<bool> Handle(ConsoleCommandEvent<EchoConsoleCommand> request, CancellationToken cancellationToken)
        {
            var text = request.Command.Text ?? "+++ hello from echo console command";
            if (request.Command.Timestamp)
            {
                text += $" {DateTime.UtcNow.ToEpoch()}";
            }

            await Task.Run(() => this.logger.LogInformation(text));
            return true;
        }
    }
}

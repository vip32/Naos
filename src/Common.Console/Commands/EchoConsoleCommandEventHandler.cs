namespace Naos.Core.Common.Console
{
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
            await Task.Run(() => this.logger.LogInformation(request.Command.Text ?? "+++ hello from echo console command"));
            return true;
        }
    }
}

namespace Naos.Core.Common.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class ExitConsoleCommandEventHandler : ConsoleCommandEventHandler<ExitConsoleCommand>
    {
        public override Task<bool> Handle(ConsoleCommandEvent<ExitConsoleCommand> request, CancellationToken cancellationToken)
        {
            Environment.Exit((int)ExitCode.Termination);

            return Task.FromResult(true);
        }
    }
}

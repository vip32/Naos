namespace Naos.Core.Common.Console
{
    using System.Threading;
    using System.Threading.Tasks;

    public class LogoConsoleCommandEventHandler : ConsoleCommandEventHandler<LogoConsoleCommand>
    {
        public override Task<bool> Handle(ConsoleCommandEvent<LogoConsoleCommand> request, CancellationToken cancellationToken)
        {
            Console2.WriteTextLogo();

            return Task.FromResult(true);
        }
    }
}

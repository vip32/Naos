namespace Naos.Core.Common.Console
{
    using System.Threading;
    using System.Threading.Tasks;

    public class ClearScreenConsoleCommandEventHandler : ConsoleCommandEventHandler<ClearScreenConsoleCommand>
    {
        public override async Task<bool> Handle(ConsoleCommandEvent<ClearScreenConsoleCommand> request, CancellationToken cancellationToken)
        {
            await Task.Run(() => System.Console.Clear());
            return true;
        }
    }
}

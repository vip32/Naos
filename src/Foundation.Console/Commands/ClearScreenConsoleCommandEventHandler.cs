namespace Naos.Foundation
{
    using System.Threading;
    using System.Threading.Tasks;

    public class ClearScreenConsoleCommandEventHandler : ConsoleCommandEventHandler<ClearScreenConsoleCommand>
    {
        public override Task<bool> Handle(ConsoleCommandEvent<ClearScreenConsoleCommand> request, CancellationToken cancellationToken)
        {
            System.Console.Clear();

            return Task.FromResult(true);
        }
    }
}

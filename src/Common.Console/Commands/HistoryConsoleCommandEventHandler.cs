namespace Naos.Foundation
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Console = Colorful.Console;

    public class HistoryConsoleCommandEventHandler : ConsoleCommandEventHandler<HistoryConsoleCommand>
    {
        public override Task<bool> Handle(ConsoleCommandEvent<HistoryConsoleCommand> request, CancellationToken cancellationToken)
        {
            foreach(var item in ReadLine.GetHistory().Distinct())
            {
                Console.WriteLine(item, Color.Gray);
            }

            return Task.FromResult(true);
        }
    }
}

namespace Naos.Core.Common.Console
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Console = Colorful.Console;

    public class ShowHistoryConsoleCommandEventHandler : ConsoleCommandEventHandler<ShowHistoryConsoleCommand>
    {
        public override async Task<bool> Handle(ConsoleCommandEvent<ShowHistoryConsoleCommand> request, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                foreach (var item in ReadLine.GetHistory())
                {
                    Console.WriteLine(item, Color.Gray);
                }
            });
            return true;
        }
    }
}

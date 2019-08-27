namespace Naos.Foundation
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Console = Colorful.Console;

    public class ExitConsoleCommandEventHandler : ConsoleCommandEventHandler<ExitConsoleCommand>
    {
        public override Task<bool> Handle(ConsoleCommandEvent<ExitConsoleCommand> request, CancellationToken cancellationToken)
        {
            this.SaveHistory();

            Environment.Exit((int)ExitCode.Termination);

            return Task.FromResult(true);
        }

        private void SaveHistory()
        {
            // save history for later use
            var directory = Path.Combine(Path.GetTempPath(), "naos_console");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new StreamWriter(
                File.Create(Path.Combine(directory, "history.db"))))
            {
                Console.WriteLine("saving history", Color.Gray);
                foreach (var history in ReadLine.GetHistory().Distinct())
                {
                    if (!history.IsNullOrEmpty() && !history.EqualsAny(new[] { "exit" }))
                    {
                        writer.WriteLine(history);
                    }
                }
            }
        }
    }
}

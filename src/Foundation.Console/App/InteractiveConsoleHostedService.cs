namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using CommandLine.Text;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Console = Colorful.Console;

    public class InteractiveConsoleHostedService : IHostedService
    {
        private readonly ILogger<InteractiveConsoleHostedService> logger;
        private readonly IEnumerable<IConsoleCommand> commands;
        private readonly IMediator mediator;

        public InteractiveConsoleHostedService(
            ILoggerFactory loggerFactory,
            IMediator mediator,
            IEnumerable<IConsoleCommand> commands)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.logger = loggerFactory.CreateLogger<InteractiveConsoleHostedService>();
            this.mediator = mediator;
            this.commands = commands.Safe();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("\r\n--- naos interactive console start", Color.LimeGreen);

            Task.Run(() => this.Run(), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task Run()
        {
            //Thread.Sleep(500);
            await Task.Delay(500).AnyContext();

            foreach (var command in this.commands)
            {
                Console.WriteLine($"found command: {command.GetType().GetAttributeValue<VerbAttribute, string>(a => a.Name) ?? "?NAME?"} ({command.GetType()})", Color.Gray);
            }

            using (var parser = new Parser())
            {
                ReadLine.AutoCompletionHandler = new AutoCompletionHandler();
                ReadLine.HistoryEnabled = true;
                var path = Path.Combine(Path.GetTempPath(), "naos_console", "history.db");
                if (File.Exists(path))
                {
                    ReadLine.AddHistory(File.ReadAllLines(path));
                }

                var originalColor = System.Console.ForegroundColor;
                while (true)
                {
                    //Thread.Sleep(500);
                    await Task.Delay(500).AnyContext();
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                    var inputLine = ReadLine.Read("naos> ").Trim();
                    System.Console.ForegroundColor = originalColor;

                    if (!inputLine.IsNullOrEmpty())
                    {
                        foreach (var input in inputLine.Split('|'))
                        {
                            try
                            {
                                var result = parser.ParseArguments(
                                    ArgumentsHelper.Split(input.Trim()),
                                    this.commands.Select(c => c.GetType()).ToArray())
                                        .WithNotParsed(_ =>
                                        {
                                            if (!input.Contains("--help"))
                                            {
                                                Console.WriteLine("invalid command", Color.Red);
                                            }
                                        });

                                if (!result.TypeInfo.Current.GetAttributeValue<VerbAttribute, string>(a => a.Name).IsNullOrEmpty())
                                {
                                    // command found
                                    var command = (result as Parsed<object>)?.Value;
                                    if (command != null)
                                    {
                                        // send the command so it can be handled by a command handler
                                        await command.As<IConsoleCommand>().SendAsync(this.mediator).AnyContext();
                                    }
                                    else
                                    {
                                        // command cannot be parsed, invalid options. show help
                                        Console.WriteLine(
                                            new HelpText { AddDashesToOption = true, AutoHelp = false, AutoVersion = false }
                                                .AddPreOptionsLine(result.TypeInfo.Current.GetAttributeValue<VerbAttribute, string>(a => a.HelpText) ?? string.Empty)
                                                .AddPreOptionsLine($"Usage: {result.TypeInfo.Current.GetAttributeValue<VerbAttribute, string>(a => a.Name)} [OPTIONS]")
                                                //.AddPostOptionsLine($"({result.TypeInfo.Current.PrettyName()})")
                                                .AddVerbs(this.commands.Select(c => c.GetType()).ToArray())
                                                .AddOptions(result),
                                            Color.Gray);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(); // no command found
                                }
                            }
#pragma warning disable CA1031 // Do not catch general exception types
                            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                            {
                                Console.WriteLine($"[{ex.GetType().PrettyName()}] {ex.GetFullMessage()}", Color.Red);
                                Console.WriteLine($"{ex.StackTrace}", Color.Red);
                            }
                        }
                    }
                }
            }
        }

        private sealed class AutoCompletionHandler : IAutoCompleteHandler // TODO: let all ConsoleCommands fill this (via reflection/helptext?)
        {
            // characters to start completion from
            public char[] Separators { get; set; } = new char[] { ' ', '.', '/' };

            // text - The current text entered in the console
            // index - The index of the terminal cursor within {text}
            public string[] GetSuggestions(string text, int index)
            {
                if (text.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
                {
                    return new string[] { "-t", "-s" };
                }
                else if (text.StartsWith("messaging ", StringComparison.OrdinalIgnoreCase))
                {
                    return new string[] { "--echo" };
                }
                else if (text.StartsWith("queueing ", StringComparison.OrdinalIgnoreCase))
                {
                    return new string[] { "--echo" };
                }
                else if (text.StartsWith("jobscheduler ", StringComparison.OrdinalIgnoreCase))
                {
                    return new string[] { "--enable", "--disable", "--trigger" };
                }
                else
                {
                    return null;
                }
            }
        }
    }
}

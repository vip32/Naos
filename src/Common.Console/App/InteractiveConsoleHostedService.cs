namespace Naos.Core.Common.Console.App
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("\r\nnaos interactive console start", Color.LimeGreen);

            foreach (var command in this.commands)
            {
                Console.WriteLine($"found command: {command.GetType().GetAttributeValue<VerbAttribute, string>(a => a.Name) ?? "?NAME?"} ({command.GetType()})", Color.Gray);
            }

            Thread.Sleep(500);
            var parser = new Parser();
            ReadLine.HistoryEnabled = true;
            ReadLine.AutoCompletionHandler = new AutoCompletionHandler();
            var originalColor = System.Console.ForegroundColor;
            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                string input = ReadLine.Read("naos> ").Trim();
                System.Console.ForegroundColor = originalColor;

                if (input.EqualsAny(new[] { "exit", "quit", "q" }))
                {
                    Environment.Exit((int)ExitCode.Termination); // TODO: move to a ConsoleCommand, also does not really exit the console
                }

                if (!input.IsNullOrEmpty())
                {
                    try
                    {
                        var result = parser.ParseArguments(
                            ArgumentsHelper.Split(input),
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
                            // no command found
                            Console.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{ex.GetType().PrettyName()}] {ex.GetFullMessage()}", Color.Red);
                        Console.WriteLine($"{ex.StackTrace}", Color.Red);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //Console.WriteLine("hosted service stopping", Color.Gray);

            return Task.CompletedTask;
        }

        public class AutoCompletionHandler : IAutoCompleteHandler // TODO: let all ConsoleCommands fill this (via reflection/helptext?)
        {
            // characters to start completion from
            public char[] Separators { get; set; } = new char[] { ' ', '.', '/' };

            // text - The current text entered in the console
            // index - The index of the terminal cursor within {text}
            public string[] GetSuggestions(string text, int index)
            {
                if (text.StartsWith("echo "))
                {
                    return new string[] { "-t", "-s" };
                }
                else if (text.StartsWith("messaging "))
                {
                    return new string[] { "--echo" };
                }
                else if (text.StartsWith("queueing "))
                {
                    return new string[] { "--echo" };
                }
                else if (text.StartsWith("jobscheduler "))
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

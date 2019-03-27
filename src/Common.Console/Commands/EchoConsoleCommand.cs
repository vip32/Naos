namespace Naos.Core.Common.Console
{
    using CommandLine;

    [Verb("echo", HelpText = "Echo a text message")]
    public class EchoConsoleCommand : IConsoleCommand
    {
        [Option('t', "text", HelpText = "Use the given text as the echo message", Required = true)]
        public string Text { get; set; }
    }
}

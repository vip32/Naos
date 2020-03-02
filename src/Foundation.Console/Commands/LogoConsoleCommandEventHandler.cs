namespace Naos.Foundation
{
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    public class LogoConsoleCommandEventHandler : ConsoleCommandEventHandler<LogoConsoleCommand>
    {
        private readonly ServiceDescriptor serviceDescriptor;

        public LogoConsoleCommandEventHandler(ServiceDescriptor serviceDescriptor)
        {
            this.serviceDescriptor = serviceDescriptor;
        }

        public override Task<bool> Handle(ConsoleCommandEvent<LogoConsoleCommand> request, CancellationToken cancellationToken)
        {
            Console2.WriteTextLogo();
            Colorful.Console.Write($"    {this.serviceDescriptor}", Color.White);
            Colorful.Console.WriteLine($" [{this.serviceDescriptor.Tags.ToString("|")}]", Color.Gray);
            Colorful.Console.WriteLine();

            return Task.FromResult(true);
        }
    }
}

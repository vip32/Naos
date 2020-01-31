namespace Naos.Foundation.Application
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using static System.Runtime.InteropServices.OSPlatform;
    using static System.Runtime.InteropServices.RuntimeInformation;
    using Console = Colorful.Console;

    public class OpenBrowserConsoleCommandEventHandler : ConsoleCommandEventHandler<OpenBrowserConsoleCommand>
    {
        private readonly IServer server;

        public OpenBrowserConsoleCommandEventHandler(IServer server)
        {
            this.server = server;
        }

        public override Task<bool> Handle(ConsoleCommandEvent<OpenBrowserConsoleCommand> request, CancellationToken cancellationToken)
        {
            if (this.server != null)
            {
                var url = this.server?.Features?.Get<IServerAddressesFeature>()?.Addresses?.First();

                if (request.Command.Logs)
                {
                    url += "/naos/operations/logevents/dashboard";
                }
                else if (request.Command.Traces)
                {
                    url += "/naos/operations/logtraces/dashboard";
                }
                else if (request.Command.Journal)
                {
                    url += "/naos/operations/logevents/dashboard?q=TrackType=journal";
                }
                else if (request.Command.Swagger)
                {
                    url += "/swagger/index.html";
                }

                Console.WriteLine($"opening browser: {url}", Color.Gray);

                var browser =
                    IsOSPlatform(Windows) ? new ProcessStartInfo("cmd", $"/c start {url}") :
                    IsOSPlatform(OSX) ? new ProcessStartInfo("open", url) :
                    new ProcessStartInfo("xdg-open", url); //linux

                Process.Start(browser);
            }

            return Task.FromResult(true);
        }
    }
}

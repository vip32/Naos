namespace Naos.Application.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Naos.Core.Configuration.App;
    using Serilog;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((context, config) =>
                //    NaosConfigurationFactory.Extend(config, args, context.HostingEnvironment.EnvironmentName))

                //.UseUrls($"https://localhost:{GetNextAvailablePort()}")
                //.CaptureStartupErrors(true)
                .UseStartup<Startup>()
                .UseSerilog();

        //private static int GetNextAvailablePort()
        //{
        //    var l = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        //    var port = 0;
        //    try
        //    {
        //        l.Start();
        //        port = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
        //        l.Stop();
        //        l.Server.Dispose();
        //    }
        //    catch
        //    { /*do nothing */
        //    }

        //    return port;
        //}
    }
}

namespace Naos.Core.Sample.Messaging.App.Console
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IHostedService, MessagingTestHostedService>();
                });

            await builder.RunConsoleAsync();
        }
    }
}

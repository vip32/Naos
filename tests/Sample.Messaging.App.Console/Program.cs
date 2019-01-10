namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Naos.Core.App.Configuration;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = NaosConfigurationFactory.CreateRoot();
            string[] capabilities = { $"{AppDomain.CurrentDomain.FriendlyName}-A", $"{AppDomain.CurrentDomain.FriendlyName}-B", $"{AppDomain.CurrentDomain.FriendlyName}-C" };

            var builder = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IHostedService, MessagingTestHostedService>()
                            .AddNaosOperationsSerilog(configuration)
                            .AddNaosMessagingFileSystem(configuration);
                            //.AddNaosMessagingAzureServiceBus(
                            //    configuration,
                            //    subscriptionName: capabilities[new Random().Next(0, capabilities.Length)]);
                });

            await builder.RunConsoleAsync();
        }
    }
}

namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Configuration;
    using Naos.Core.RequestCorrelation.App.Web;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = NaosConfigurationFactory.Create();
            string[] capabilities = { $"{AppDomain.CurrentDomain.FriendlyName}-A", $"{AppDomain.CurrentDomain.FriendlyName}-B", $"{AppDomain.CurrentDomain.FriendlyName}-C" };

            var builder = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    // framework application services
                    services.AddTransient<HttpClientLogHandler>();
                    services.AddTransient<HttpClientCorrelationHandler>();
                    services.AddHttpClient("default")
                        .AddHttpMessageHandler<HttpClientCorrelationHandler>()
                        .AddHttpMessageHandler<HttpClientLogHandler>();
                    //services.Replace(ServiceDescriptor.Singleton<Microsoft.Extensions.Http.IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());
                    services.RemoveAll<Microsoft.Extensions.Http.IHttpMessageHandlerBuilderFilter>();
                    services.AddMediatR();

                    // naos application services
                    services
                        .AddNaosRequestCorrelation()
                        .AddNaosOperationsSerilog(configuration, correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}")
                        //.AddNaosMessagingSignalR(configuration)
                        //.AddNaosMessagingFileSystem(configuration)
                        .AddNaosMessagingAzureServiceBus(
                            configuration,
                            subscriptionName: capabilities[new Random().Next(0, capabilities.Length)])
                        .AddSingleton<IHostedService, MessagingTestHostedService>();
                });

            await builder.RunConsoleAsync();
        }
    }
}

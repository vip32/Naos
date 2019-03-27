namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Naos.Core.Common;
    using Naos.Core.Common.Web;
    using Naos.Core.Configuration.App;
    using Naos.Core.JobScheduling.App;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Core.RequestCorrelation.App.Web;
    using Console = Colorful.Console;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            CommonConsole.WriteNaosTextLogo();
            var configuration = NaosConfigurationFactory.Create();
            string[] capabilities = { "CapabilityA", "CapabilityB", "CapabilityC" };

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
                        .AddNaos(configuration, "Product", capabilities[new Random().Next(0, capabilities.Length)], new[] { "All" }, n => n
                            .AddRequestCorrelation()
                            .AddOperations(o => o
                                .AddLogging(l => l
                                    .UseConsole()
                                    .UseFile()
                                    .UseAzureLogAnalytics(),
                                    correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}"))
                            .AddJobScheduling(o => o
                                .Register<EchoJob>("testjob1", Cron.Minutely(), (j) => j.EchoAsync("+++ hello from testjob1 +++", CancellationToken.None))
                                .Register("jobevent1", Cron.Minutely(), () => new EchoJobEventData { Text = "+++ hello from jobevent1 +++" }))
                            .AddQueueing()
                            .AddMessaging(o => o
                                //.UseFileSystemBroker()));
                                //.UseSignalRBroker()));
                                //.UseRabbitMQBroker()));
                                .UseServiceBusBroker()));

                    //services
                    //    .AddSingleton<IHostedService, ConsoleHostedService>();
                });

            //Console.WriteLine($"webapp starting ({typeof(Naos.Sample.App.Web.Program).FullPrettyName()})", Color.Gray);
            //await Naos.Sample.App.Web.Program.CreateWebHostBuilder(null).Build().StartAsync();
            //await Naos.Sample.App.Web.Program.CreateWebHostBuilder(args).Build().RunAsync();
            //Console.WriteLine($"webapp ready ({typeof(Naos.Sample.App.Web.Program).FullPrettyName()})\r\n", Color.Gray);

            Console.WriteLine("console starting", Color.Gray);
            await builder.RunConsoleAsync();
        }
    }
}

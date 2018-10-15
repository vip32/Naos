namespace Naos.Core.Sample.Messaging.App.Console
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Naos.Core.Common;
    using Naos.Core.Infrastructure.Azure.KeyVault;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Serilog;
    using Serilog.Events;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", AppDomain.CurrentDomain.FriendlyName)
                .WriteTo.Debug()
                .WriteTo.LiterateConsole(/*outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}|{Level} => {CorrelationId} => {Service}::{SourceContext}{NewLine}    {Message}{NewLine}{Exception}"*/)
                .CreateLogger();

            try
            {
                Log.Information("starting host");

                var builder = new HostBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: true);
                        config.AddEnvironmentVariables();
                        if (args != null)
                        {
                            config.AddCommandLine(args);
                        }

                        var builtConfig = config.Build();

                        if (builtConfig["naos:secrets:vault:enabled"].ToBool(true)
                            && !string.IsNullOrEmpty(builtConfig["naos:secrets:vault:name"]))
                        {
                            config.AddAzureKeyVault(
                                $"https://{builtConfig["naos:secrets:vault:name"]}.vault.azure.net/",
                                builtConfig["naos:secrets:vault:clientId"],
                                builtConfig["naos:secrets:vault:clientSecret"],
                                new EnvironmentPrefixKeyVaultSecretManager());
                        }
                    })
                    .ConfigureServices((context, services) =>
                    {
                        string[] capabilities = { $"{AppDomain.CurrentDomain.FriendlyName}-A", $"{AppDomain.CurrentDomain.FriendlyName}-B", $"{AppDomain.CurrentDomain.FriendlyName}-C" };

                        services.AddOptions();
                        //services.Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"));
                        services.AddSingleton<IHostedService, PublishHostedService>();
                        services.AddNaosMessaging(context.Configuration, subscriptionName: capabilities[new Random().Next(0, capabilities.Length)]);
                        // register all message handlers
                        services.AddScoped(typeof(TestMessageHandler));
                        services.AddScoped(typeof(EntityMessageHandler<>));
                        services.AddScoped(typeof(StubEntityMessageHandler));
                    })
                    .UseSerilog();

                await builder.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "host terminated unexpectedly");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
